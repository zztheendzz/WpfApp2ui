using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.model;

namespace WpfApp2.Services
{
    public class VendorService:BaseService<Vendor>
    {
        private DatabaseService _db = new DatabaseService();

        public IEnumerable<Vendor> Search(string keyword)
        {
            using var conn = _db.GetConnection();
            return conn.Query<Vendor>(
                "SELECT * FROM Vendor WHERE VendorName LIKE @key",
                new { key = "%" + keyword + "%" });
        }
    }
}
