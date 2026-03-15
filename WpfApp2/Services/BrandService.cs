using Dapper;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.model;

namespace WpfApp2.Services
{

    namespace WpfApp2.Services
    {
        public class BrandService:BaseService<Brand>

        {
            public DatabaseService _db = new DatabaseService();

            public IEnumerable<Brand> Search(string keyword)
            {
                using var conn = _db.GetConnection();
                return conn.Query<Brand>(
                    "SELECT * FROM Brand WHERE BrandName LIKE @key",
                    new { key = "%" + keyword + "%" });
            }


        }
    }
}

