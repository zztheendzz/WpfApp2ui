using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.model;
using WpfApp2.modelDto;
using WpfApp2.modelDTO;

namespace WpfApp2.Services
{
    public class SearchService
    {
        private readonly DatabaseService _db = new ();

        // ================= GLOBAL SEARCH =================
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

        // ================= PURCHASE SEARCH (MAIN) =================
        public IEnumerable<PurchaseDto> SearchPurchase(
            string model,
            string vendor,
            int? equipmentId,
            DateTime? from,
            DateTime? to,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var sql = new StringBuilder(@"
                SELECT 
                    p.Id,
                    m.ModelName,
                    v.VendorName,
                    e.EquipmentName,
                    p.Quantity,
                    p.UnitPrice,
                    p.Quantity * p.UnitPrice AS TotalPrice,
                    p.CurrencyCode,
                    p.PurchaseDate,
                    p.Note
                FROM Purchase p
                LEFT JOIN Model m ON p.ModelId = m.Id
                LEFT JOIN Vendor v ON p.VendorId = v.Id
                LEFT JOIN Equipment e ON p.EquipmentId = e.Id

                WHERE 1=1
            ");

            var param = new DynamicParameters();

            // 🔍 Model search (text)
            if (!string.IsNullOrWhiteSpace(model))
            {
                string pattern = "%" + string.Join("%", model.ToLower().Trim().ToCharArray()) + "%";
                sql.Append(" AND LOWER(m.ModelName) LIKE @model");
                param.Add("model", pattern);
            }

            // 🔍 Vendor search (text)
            if (!string.IsNullOrWhiteSpace(vendor))
            {
                string pattern = "%" + string.Join("%", vendor.ToLower().Trim().ToCharArray()) + "%";
                sql.Append(" AND LOWER(v.VendorName) LIKE @vendor");
                param.Add("vendor", pattern);
            }

            // 🎯 Filter khác
            if (equipmentId.HasValue)
                sql.Append(" AND p.EquipmentId = @equipmentId");


            if (from.HasValue)
                sql.Append(" AND p.PurchaseDate >= @from");

            if (to.HasValue)
                sql.Append(" AND p.PurchaseDate <= @to");

            if (minPrice.HasValue)
                sql.Append(" AND p.UnitPrice >= @minPrice");

            if (maxPrice.HasValue)
                sql.Append(" AND p.UnitPrice <= @maxPrice");

            sql.Append(" ORDER BY p.PurchaseDate DESC");

            using var conn = _db.GetConnection();

            return conn.Query<PurchaseDto>(sql.ToString(), new
            {
                model = param.ParameterNames.Contains("model") ? param.Get<string>("model") : null,
                vendor = param.ParameterNames.Contains("vendor") ? param.Get<string>("vendor") : null,
                equipmentId,
                from,
                to,
                minPrice,
                maxPrice
            });
        }

        // ================= BRAND =================
        public IEnumerable<SearchResultDto> SearchBrand(string keyword)
        {
            using var conn = _db.GetConnection();
            string pattern = "%" + keyword + "%";

            var brands = conn.Query<Brand>(
                "SELECT * FROM Brand WHERE BrandName LIKE @pattern LIMIT 20",
                new { pattern });

            return brands.Select(b => new SearchResultDto
            {
                Id = b.Id,
                Source = "Brand",
                Text = b.BrandName,
                Data = b
            });
        }

        // ================= VENDOR =================
        public IEnumerable<SearchResultDto> SearchVendor(string keyword)
        {
            using var conn = _db.GetConnection();
            string pattern = "%" + keyword + "%";

            var vendors = conn.Query<Vendor>(
                "SELECT * FROM Vendor WHERE VendorName LIKE @pattern LIMIT 20",
                new { pattern });

            return vendors.Select(v => new SearchResultDto
            {
                Id = v.Id,
                Source = "Vendor",
                Text = v.VendorName,
                Data = v
            });
        }

        // ================= EQUIPMENT =================
        public IEnumerable<SearchResultDto> SearchEquipment(string keyword)
        {
            using var conn = _db.GetConnection();
            string pattern = "%" + keyword + "%";

            var equipment = conn.Query<Equipment>(
                "SELECT * FROM Equipment WHERE EquipmentName LIKE @pattern LIMIT 20",
                new { pattern });

            return equipment.Select(e => new SearchResultDto
            {
                Id = e.Id,
                Source = "Equipment",
                Text = e.EquipmentName,
                Data = e
            });
        }

        // ================= MODEL =================
        public IEnumerable<SearchResultDto> SearchModel(string keyword)
        {
            using var conn = _db.GetConnection();
            string pattern = "%" + keyword + "%";

            var models = conn.Query<Model>(
                "SELECT * FROM Model WHERE ModelName LIKE @pattern LIMIT 20",
                new { pattern });

            return models.Select(m => new SearchResultDto
            {
                Id = m.Id,
                Source = "Model",
                Text = m.ModelName,
                Data = m
            });
        }
    }
}