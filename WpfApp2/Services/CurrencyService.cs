using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.model;

namespace WpfApp2.Services
{
    public class CurrencyService:BaseService<Currency>
    {
        private DatabaseService _db = new DatabaseService();

        public IEnumerable<Currency> Search(string keyword)
        {
            using var conn = _db.GetConnection();
            return conn.Query<Currency>(
                "SELECT * FROM Currency WHERE CurrencyName LIKE @key",
                new { key = "%" + keyword + "%" });
        }
    }
}
