using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;

namespace WpfApp2.Services.analysisService
{
    public class VendorAnalysisSv
    {
        public DatabaseService _db = new DatabaseService();

        public VendorAnalysisDto GetVendorAnalysis(int vendorId)
        {
            using var conn = _db.GetConnection();

            string sql = @"
-- Summary
SELECT
    IFNULL(SUM(p.Quantity), 0) AS TotalQuantity,
    IFNULL(SUM(p.Quantity * p.UnitPrice), 0) AS TotalPrice,
    COUNT(p.Id) AS SupplyCount,
    COUNT(DISTINCT p.EquipmentId) AS TotalEquipment
FROM PurchaseHistory p
WHERE p.VendorId = @vendorId;

-- List
SELECT
    p.Id,

    m.ModelName        AS ModelName,   -- 🔥 linh kiện (chính)
    e.EquipmentName    AS EquipmentName,   -- 🔥 thiết bị cha

    b.BrandName,

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

LEFT JOIN [User] u ON p.UserId = u.Id

WHERE p.VendorId = @vendorId

ORDER BY p.PurchaseDate DESC;
";

            using var multi = conn.QueryMultiple(sql, new { vendorId });

            var summary = multi.ReadFirst<VendorAnalysisDto>();
            var items = multi.Read<PurchaseDto>().ToList();

            summary.Items = items;

            return summary;
        }
    }
}