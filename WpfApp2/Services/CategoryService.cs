using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDTO;

namespace WpfApp2.Services
{
    internal class CategoryService
    {
        public DatabaseService _db = new DatabaseService();
        public IEnumerable<CategoryDto> GetCategoryDTO()
        {
            using var conn = _db.GetConnection();

            string sql = @"
SELECT 
    m.Id,
    m.CategoryName,
    m.ParentId,
    CASE 
        WHEN m.IsActive = 1 THEN 1 
        ELSE 0 
    END AS IsActive
FROM Category m
    ";
            return conn.Query<CategoryDto>(sql);
        }


        public void Delete(int id)
        {
            using var conn = _db.GetConnection();

            string sql = "DELETE FROM Category WHERE Id = @Id";

            conn.Execute(sql, new { Id = id });
        }

        public void Edit(CategoryDto category)
        {
            using var conn = _db.GetConnection();

            string sql = @"
        UPDATE Category
        SET 
            CategoryName = @ModelCode,
            ParentId = @ParentId,
            IsActive = @IsActive
        WHERE Id = @Id
        ";

            conn.Execute(sql, category);
        }

        public int Add(CategoryDto category)
        {
            using var conn = _db.GetConnection();

            string sql = @"
    INSERT INTO Category (CategoryName, ParentId, IsActive)
    VALUES (@CategoryName, @ParentId, @IsActive);

    SELECT last_insert_rowid();
    ";
            return conn.ExecuteScalar<int>(sql, category);
        }

    }
}