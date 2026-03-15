using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.Services
{
    using Dapper.Contrib.Extensions;

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
    }
}
