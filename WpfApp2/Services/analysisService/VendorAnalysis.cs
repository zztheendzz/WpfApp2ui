using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;

namespace WpfApp2.Services.analysisService
{
    public class VendorAnalysis
    {
        public DatabaseService _db = new DatabaseService();

        public VendorAnalysisDto GetBrandAnalysis(int VendorId)
        {
            using var conn = _db.GetConnection();

            string sql = @"
-- Summary
SELECT
    v.Id AS VendorId,
    v.VendorName,

    COUNT(p.Id) AS SupplyCount, 

    IFNULL(SUM(p.Quantity * p.UnitPrice), 0) AS TotalAmount,

    GROUP_CONCAT(DISTINCT m.ModelName) AS ModelList -- danh sách linh kiện (Model)
FROM PurchaseHistory p
JOIN Vendor v ON p.VendorId = v.Id
JOIN Model m ON p.ModelId = m.Id
GROUP BY v.Id, v.VendorName
ORDER BY TotalAmount DESC;

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
    p.CreateAt, -- sửa lại tên cho đúng DB của bạn
    u.UserName,
    p.Note
FROM PurchaseHistory p
LEFT JOIN Model m ON p.ModelId = m.Id
LEFT JOIN Vendor v ON p.VendorId = v.Id
LEFT JOIN Equipment e ON p.EquipmentId = e.Id
LEFT JOIN Category c ON m.CategoryId = c.Id
LEFT JOIN [User] u ON p.UserId = u.Id
WHERE p.VendorId = @vendorId
ORDER BY p.PurchaseDate DESC;
";

            using var multi = conn.QueryMultiple(sql, new { VendorId });

            var summary = multi.ReadFirst<VendorAnalysisDto>();
            var items = multi.Read<PurchaseDto>().ToList();

            summary.Items = items;

            return summary;
        }
    }
}