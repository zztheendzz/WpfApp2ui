using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDto;
namespace WpfApp2.Services
{
    internal class BrandService
    {

        public DatabaseService _db = new DatabaseService();
        public IEnumerable<BrandDto> GetBrandDTO()
        {
            using var conn = _db.GetConnection();

            string sql = @"
                SELECT 
                    m.Id,
                    m.BrandName,
                    m.IsActive
                    FROM Brand m
                WHERE IsActive = 1
                    ";
            return conn.Query<BrandDto>(sql);
        }


        public void Delete(int id)
        {
            using var conn = _db.GetConnection();

            string sql = " UPDATE Brand SET IsActive = 0 WHERE Id = @Id";

            conn.Execute(sql, new { Id = id });
        }

        public void Edit(BrandDto brand)
        {
            using var conn = _db.GetConnection();

            string sql = @"
        UPDATE Brand
        SET 
            Brandname = @BrandName
        WHERE Id = @Id
        ";

            conn.Execute(sql, brand);
        }

        public int Add(BrandDto brand)
        {
            using var conn = _db.GetConnection();

            string sql = @"
    INSERT INTO Brand (BrandName, IsActive)
    VALUES (@BrandName, @IsActive);

    SELECT last_insert_rowid();
    ";
            return conn.ExecuteScalar<int>(sql, brand);
        }

    }
}