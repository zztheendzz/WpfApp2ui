using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.Services
{
    using Dapper;
    using Dapper.Contrib.Extensions;
    using global::WpfApp2.model;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Reflection;

    public class BaseService<T> where T : class
    {
        public DatabaseService _db = new DatabaseService();

        public IEnumerable<T> GetAll()
        {
            using var conn = _db.GetConnection();
            return conn.GetAll<T>();
        }

        public T Get(int id)
        {
            using var conn = _db.GetConnection();
            return conn.Get<T>(id);
        }

        public long Add(T model)
        {
            using var conn = _db.GetConnection();
            return conn.Insert(model);
        }

        public bool Update(T model)
        {
            using var conn = _db.GetConnection();
            return conn.Update(model);
        }

        public bool Delete(T model)
        {
            using var conn = _db.GetConnection();
            return conn.Delete(model);
        }
        public IEnumerable<T> Query(string sql, object param = null)
        {
            using var conn = _db.GetConnection();
            return conn.Query<T>(sql, param);
        }

        public T QueryFirst(string sql, object param = null)
        {
            using var conn = _db.GetConnection();
            return conn.QueryFirstOrDefault<T>(sql, param);
        }

        public int Execute(string sql, object param = null)
        {
            using var conn = _db.GetConnection();
            return conn.Execute(sql, param);
        }
        // use all table
        public IEnumerable<T> Search(string table, string column, string keyword)
        {
            using var conn = _db.GetConnection();

            string sql = $"SELECT * FROM {table} WHERE {column} LIKE @keyword";

            return conn.Query<T>(sql, new { keyword = "%" + keyword + "%" });
        }
        public IEnumerable<T> Search(string column, string keyword)
        {
            using var conn = _db.GetConnection();

            var tableAttr = typeof(T).GetCustomAttribute<TableAttribute>();
            
            string table = tableAttr.Name;

            string sql = $"SELECT * FROM {table} WHERE {column} LIKE @keyword ORDER BY Id LIMIT 10 OFFSET 0";

            return conn.Query<T>(sql, new { keyword = "%" + keyword + "%" });
        }
        //search multil column
        public IEnumerable<T> Search(string keyword, params string[] columns)
        {
            using var conn = _db.GetConnection();

            var tableAttr = typeof(T).GetCustomAttribute<TableAttribute>();
            string table = tableAttr.Name;

            var conditions = string.Join(" OR ",
                columns.Select(c => $"{c} LIKE @keyword"));

            string sql = $"SELECT * FROM {table} WHERE {conditions} LIMIT 10";

            return conn.Query<T>(sql, new { keyword = "%" + keyword + "%" });
        }
        //search all 

    }
}
