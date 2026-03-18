using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;

namespace WpfApp2.Services.analysisService
{
    public class BrandAnalysisSv
    {
        public DatabaseService _db = new DatabaseService();

        public BrandAnalysisDto GetBrandAnalysis(int brandId)
        {
            using var conn = _db.GetConnection();

            string sql = @"
-- Summary
SELECT
    IFNULL(SUM(p.Quantity), 0) AS TotalQuantity,
    IFNULL(SUM(p.Quantity * p.UnitPrice), 0) AS TotalPrice
FROM PurchaseHistory p
JOIN Model m ON p.ModelId = m.Id
WHERE m.BrandId = @brandId;

-- List
SELECT
    p.Id,
    m.ModelName,
    v.VendorName,
    e.EquipmentName,
    c.CategoryName,
    p.Quantity,
    p.UnitPrice,
    IFNULL(p.Quantity * p.UnitPrice, 0) AS TotalPrice,
    p.CurrencyCode,
    p.PurchaseDate,
    p.CreateAt,
    u.UserName,
    p.Note
FROM PurchaseHistory p
LEFT JOIN Model m ON p.ModelId = m.Id
LEFT JOIN Vendor v ON p.VendorId = v.Id
LEFT JOIN Equipment e ON p.EquipmentId = e.Id
LEFT JOIN Category c ON m.CategoryId = c.Id   -- ✅ FIX
LEFT JOIN [User] u ON p.UserId = u.Id
WHERE m.BrandId = @brandId                    -- ✅ FIX
ORDER BY m.id DESC;
";

            using var multi = conn.QueryMultiple(sql, new { brandId });

            var summary = multi.ReadFirst<BrandAnalysisDto>();
            var items = multi.Read<PurchaseDto>().ToList();

            summary.Items = items;

            return summary;
        }
    }
}