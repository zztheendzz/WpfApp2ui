using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.Services;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace WpfApp2.viewmodel.common
{
        public class BaseService<T> where T : class
        {
            protected DatabaseService _db = new DatabaseService();

            public IEnumerable<T> GetAll()
            {
                using var conn = _db.GetConnection();
                return conn.GetAll<T>();
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
