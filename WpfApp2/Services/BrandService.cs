using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDto;
using WpfApp2.Services.exception;
namespace WpfApp2.Services
{
    internal class BrandService
    {

        public DatabaseService _db = new DatabaseService();

        public IEnumerable<BrandDto> GetBrandDTO()
        {
            const string sql = @"
            SELECT 
                Id,
                BrandName,
                IsActive
            FROM Brand
            WHERE IsActive = 1
        ";

            int retry = 3;

            while (retry > 0)
            {
                try
                {
                    using var conn = _db.GetConnection();

                    return conn.Query<BrandDto>(sql).ToList(); ;
                }
                catch (SqliteException ex) when (ex.Message.Contains("locked"))
                {
                    retry--;
                    Thread.Sleep(300);
                }
            }

            // ❗ nếu bị lock nhiều lần
            throw new DatabaseLockedException();
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