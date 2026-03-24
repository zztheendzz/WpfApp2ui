using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using WpfApp2.model;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;

namespace WpfApp2.Services.analysisService
{
    public class EquipmentAnalysisSv
    {
        public DatabaseService _db = new DatabaseService();
        // Equipment equipoment
        public EquipmentAnalysisDto GetEquipmentAnalysis(int equipmentId)
        {
            using var conn = _db.GetConnection();


            string sql = @"
-- Summary
SELECT
    COUNT(DISTINCT p.ModelId) AS TotalModel, -- 🔥 số loại linh kiện
    IFNULL(SUM(p.Quantity * p.UnitPrice), 0) AS TotalPrice -- 🔥 tổng tiền
FROM PurchaseHistory p
WHERE p.EquipmentId = @equipmentId;

-- List
SELECT
    p.Id,

    m.ModelName,
    e.EquipmentName,

    b.BrandName,
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
LEFT JOIN Vendor v ON p.VendorId = v.Id
LEFT JOIN [User] u ON p.UserId = u.Id

WHERE p.EquipmentId = @equipmentId

ORDER BY p.PurchaseDate DESC;
";

            using var multi = conn.QueryMultiple(sql, new {equipmentId});

            var summary = multi.ReadFirst<EquipmentAnalysisDto>();
            var items = multi.Read<PurchaseDto>().ToList();

            summary.Items = items;
            return summary;
        }
    }
}