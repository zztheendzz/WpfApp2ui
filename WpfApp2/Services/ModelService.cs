using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
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
    m.BrandId,
    m.ModelName,
    CASE 
        WHEN m.IsActive = 1 THEN 1 
        ELSE 0 
    END AS IsActive,
    b.BrandName
FROM Model m
LEFT JOIN Brand b ON m.BrandId = b.Id
    ";
            return conn.Query<ModelDto>(sql);
        }


        public void Delete(int id)
        {
            using var conn = _db.GetConnection();

            string sql = "DELETE FROM Model WHERE Id = @Id";

            conn.Execute(sql, new { Id = id });
        }

        public void Edit(ModelDto model)
        {
            using var conn = _db.GetConnection();

            string sql = @"
        UPDATE Model
        SET 
            ModelCode = @ModelCode,
            BrandId = @BrandId,
            IsActive = @IsActive
        WHERE Id = @Id
        ";

            conn.Execute(sql, model);
        }
    
    public int Add(ModelDto model)
        {
            using var conn = _db.GetConnection();

            string sql = @"
    INSERT INTO Model (ModelCode, BrandId, IsActive)
    VALUES (@ModelCode, @BrandId, @IsActive);

    SELECT last_insert_rowid();
    ";
            return conn.ExecuteScalar<int>(sql, model);
        }

    } 
}