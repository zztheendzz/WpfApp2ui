using Dapper;
using Dapper.Contrib.Extensions;
using LiveCharts.Configurations;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.model;
using Dapper.Contrib.Extensions;
namespace WpfApp2.Services
{



    public class ModelService
    {
        private DatabaseService _db = new DatabaseService();


        public IEnumerable<Model> GetPaged(string keyword)
        {
            using var conn = _db.GetConnection();

            string sql = @"
    SELECT *
    FROM Model
    WHERE ModelCode LIKE @Key
       OR ModelName LIKE @Key
    ORDER BY ModelName
    ";

            return conn.Query<Model>(sql, new
            {
                Key = "%" + keyword + "%"
            });
        }

        public IEnumerable<Model> Search(string code, int? brandId)
        {
            using var conn = _db.GetConnection();

            string sql = "SELECT * FROM Model WHERE 1=1";

            if (!string.IsNullOrEmpty(code))
                sql += " AND ModelCode LIKE @Code";

            if (brandId != null)
                sql += " AND BrandId = @BrandId";

            return conn.Query<Model>(sql, new
            {
                Code = "%" + code + "%",
                BrandId = brandId
            });
        }


        public IEnumerable<Model> Search(string code, int brandId)
        {
            using var conn = _db.GetConnection();

            string sql = @"SELECT * 
                   FROM Model
                   WHERE ModelCode LIKE @Code
                   AND BrandId = @BrandId";

            return conn.Query<Model>(sql, new
            {
                Code = "%" + code + "%",
                BrandId = brandId
            });
        }

        public IEnumerable<Model> GetAll()
        {
            using var conn = _db.GetConnection();
            return conn.GetAll<Model>();
        }

        public Model Get(int id)
        {
            using var conn = _db.GetConnection();
            return conn.Get<Model>(id);
        }

        public long Add(Model model)
        {
            using var conn = _db.GetConnection();
            return conn.Insert(model);
        }

        public bool Update(Model model)
        {
            using var conn = _db.GetConnection();
            return conn.Update(model);
        }

        public bool Delete(Model model)
        {
            using var conn = _db.GetConnection();
            return conn.Delete(model);
        }
    }
}
        
