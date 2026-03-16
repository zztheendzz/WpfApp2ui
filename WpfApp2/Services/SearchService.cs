using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.model;

namespace WpfApp2.Services
{
    public class SearchService
    {
        DatabaseService _db = new DatabaseService();

        public IEnumerable<SearchResult> GlobalSearch(string keyword)
        {
            using var conn = _db.GetConnection();

            string pattern = "%" + string.Join("%", keyword.ToCharArray()) + "%";

            var tables = conn.Query<string>(
                "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'"
            );

            List<string> queries = new();

            foreach (var table in tables)
            {
                var columns = conn.Query<dynamic>($"PRAGMA table_info({table})").ToList();

                var idColumn = columns
                    .FirstOrDefault(c => ((string)c.name).ToLower().Contains("id"))?.name;

                if (idColumn == null)
                    continue;

                var textColumn = columns
                    .FirstOrDefault(c => ((string)c.name).ToLower().Contains("name"))?.name
                    ?? columns.First().name;

                var whereParts = columns
                    .Select(c => $"{c.name} LIKE @pattern");

                string where = string.Join(" OR ", whereParts);

                queries.Add($@"
                SELECT 
                    {idColumn} as Id,
                    {textColumn} as Text,
                    '{table}' as Source
                FROM {table}
                WHERE {where}
            ");
            }

            string sql = string.Join(" UNION ", queries) + " LIMIT 20";

            return conn.Query<SearchResult>(sql, new { pattern });
        }
    }
}
