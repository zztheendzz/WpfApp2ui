using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using WpfApp2.modelDto;
using WpfApp2.modelDTO;

namespace WpfApp2.Services
{
    internal class ModelService
    {
        public DatabaseService _db = new DatabaseService();
        public IEnumerable<ModelDto> GetModelDTO()
        {
            using var conn = _db.GetConnection();
            string sql = @"
                SELECT 
                    m.Id,
                    m.ModelCode,
                    m.BrandId,        -- FK (int)
                    m.ModelName,
                    b.BrandName       -- lấy từ bảng Brand
                FROM Model m
                LEFT JOIN Brand b ON m.BrandId = b.Id
                WHERE m.IsActive = 1
                ";
            return conn.Query<ModelDto>(sql);
        }

        private string Normalize(string input)
        {
            return new string(input
                .Trim()
                .ToLower()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }


        private Dictionary<string, int> BuildBrandLookup(IDbConnection conn)
        {
            return conn.Query<BrandDto>(@"
        SELECT Id, BrandName AS BrandName
        FROM Brand
    ")
            .ToDictionary(
                x => Normalize( x.BrandName),
                x => x.Id
            );
        }


        // =========================
        // GET OR CREATE BRAND
        // =========================
        private int GetOrCreateBrand(
            IDbConnection conn,
            Dictionary<string, int> brandDict,
            string brandName)
        {
            var key = Normalize(brandName);

            // 1. Nếu đã tồn tại → dùng luôn
            if (brandDict.TryGetValue(key, out var brandId))
                return brandId;

            // 2. Nếu chưa có → insert
            brandId = conn.ExecuteScalar<int>(@"
                INSERT INTO Brand (BrandName, IsActive)
                VALUES (@BrandName,1);
                SELECT last_insert_rowid();
            ", new { BrandName = brandName });

            // 3. update cache
            brandDict[key] = brandId;

            return brandId;
        }






        public void Delete(int id)
        {
            using var conn = _db.GetConnection();

            string sql = @"
                UPDATE Model
                SET 
                    IsActive = 0
                WHERE Id = @Id
        ";

            conn.Execute(sql, new { Id = id });
        }

        public void Edit(ModelDto model)
        {
            using var conn = _db.GetConnection();

            string sql = @"
        UPDATE Model
        SET 
            ModelCode = @ModelCode,
            BrandId = @BrandId
        WHERE Id = @Id
        ";

            conn.Execute(sql, model);
        }


        public int Addcheck(ModelDto model)
        {
            using var conn = _db.GetConnection();

            string sql = @"
    INSERT INTO Model (ModelCode, BrandId,ModelName, IsActive)
    VALUES (@ModelCode, @BrandId,@ModelName ,@IsActive);

    SELECT last_insert_rowid();
    ";
            return conn.ExecuteScalar<int>(sql, model);
        }

        // =========================
        // ADD MODEL (auto xử lý Brand)
        // =========================
        public int Add(ModelDto model)
        {
            using var conn = _db.GetConnection();

            // build cache 1 lần
            var brandDict = BuildBrandLookup(conn);

            // lấy hoặc tạo brand
            model.BrandId = GetOrCreateBrand(conn, brandDict, model.BrandName);

            string sql = @"
                INSERT INTO Model (ModelCode, BrandId, ModelName, IsActive)
                VALUES (@ModelCode, @BrandId, @ModelName, 1);

                SELECT last_insert_rowid();
            ";

            return conn.ExecuteScalar<int>(sql, model);
        }

    } 
}