using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysisDto;
using WpfApp2.modelDTO.analysysDto;
namespace WpfApp2.Services.analysisService
{
    public class ModelAnalysisSv
    {
        public DatabaseService _db = new DatabaseService();
        public ModelAnalysisDto GetModelAnalysis(int modelId)
        {
            using var conn = _db.GetConnection();

            string sql = @"
-- 1. SUMMARY
SELECT 
    m.Id AS ModelId,
    m.ModelName,

    (
        SELECT ph.UnitPrice
        FROM PurchaseHistory ph
        WHERE ph.ModelId = m.Id
        ORDER BY ph.PurchaseDate DESC
        LIMIT 1
    ) AS LatestPrice,

    IFNULL(MIN(p.UnitPrice), 0) AS MinPrice,
    IFNULL(MAX(p.UnitPrice), 0) AS MaxPrice,
    IFNULL(AVG(p.UnitPrice), 0) AS AvgPrice,

    v.Id AS BestVendorId,
    v.VendorName AS BestVendorName,
    ph_min.UnitPrice AS BestVendorPrice

FROM Model m
LEFT JOIN PurchaseHistory p ON p.ModelId = m.Id

LEFT JOIN PurchaseHistory ph_min 
    ON ph_min.ModelId = m.Id
    AND ph_min.UnitPrice = (
        SELECT MIN(p2.UnitPrice)
        FROM PurchaseHistory p2
        WHERE p2.ModelId = m.Id
    )

LEFT JOIN Vendor v ON v.Id = ph_min.VendorId

WHERE m.Id = @modelId
GROUP BY m.Id;

-- 2. VENDOR COMPARISON
SELECT
    v.Id AS VendorId,
    v.VendorName,

    IFNULL(MIN(p.UnitPrice), 0) AS MinPrice,
    IFNULL(MAX(p.UnitPrice), 0) AS MaxPrice,
    IFNULL(AVG(p.UnitPrice), 0) AS AvgPrice

FROM PurchaseHistory p
LEFT JOIN Vendor v ON v.Id = p.VendorId

WHERE p.ModelId = @modelId
GROUP BY v.Id
ORDER BY AvgPrice;

-- 3. PURCHASE HISTORY 🔥
SELECT
    p.Id,
    m.ModelName,
    e.EquipmentName,
    b.BrandName,
    c.CategoryName,
    v.VendorName,

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
LEFT JOIN Equipment e ON p.EquipmentId = e.Id
LEFT JOIN Brand b ON m.BrandId = b.Id
LEFT JOIN Category c ON m.CategoryId = c.Id
LEFT JOIN Vendor v ON p.VendorId = v.Id
LEFT JOIN [User] u ON p.UserId = u.Id

WHERE p.ModelId = @modelId

ORDER BY p.PurchaseDate DESC;
";

            using var multi = conn.QueryMultiple(sql, new { modelId });

            var summary = multi.ReadFirstOrDefault<ModelAnalysisDto>();
            var vendors = multi.Read<VendorPriceDto>().ToList();
            var history = multi.Read<PurchaseDto>().ToList();

            summary.Vendors = vendors;
            summary.Items = history;

            return summary;
        }
    }
}