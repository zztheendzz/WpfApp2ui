using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;

namespace WpfApp2.Services.analysisService
{
    public class ModelAnalysisSv
    {

        public readonly IDbConnection _db;

        public ModelAnalysisSv(IDbConnection db)
        {
            _db = db;
        }



        public (decimal lastPrice, decimal minPrice, decimal maxPrice, decimal avgPrice) GetSummary(int modelId)
        {
            string sql = @"
            SELECT 
                MIN(UnitPrice) MinPrice,
                MAX(UnitPrice) MaxPrice,
                AVG(UnitPrice) AvgPrice
            FROM PurchaseHistory
            WHERE ModelId = @modelId";

            var summary = _db.QueryFirst(sql, new { modelId });

            string lastSql = @"
            SELECT UnitPrice
            FROM PurchaseHistory
            WHERE ModelId = @modelId
            ORDER BY PurchaseDate DESC
            LIMIT 1";

            decimal lastPrice = _db.QueryFirstOrDefault<decimal>(lastSql, new { modelId });

            return (lastPrice, summary.MinPrice, summary.MaxPrice, summary.AvgPrice);
        }

        // 2. Vendor comparison
        public IEnumerable<ModelAnalysisDto> GetVendorPrices(int modelId)
        {
            string sql = @"
            SELECT
                v.VendorName,
                MIN(p.UnitPrice) MinPrice,
                MAX(p.UnitPrice) MaxPrice,
                (
                    SELECT UnitPrice
                    FROM PurchaseHistory p2
                    WHERE p2.ModelId = p.ModelId
                    AND p2.VendorId = p.VendorId
                    ORDER BY p2.PurchaseDate DESC
                    LIMIT 1
                ) LastPrice
            FROM PurchaseHistory p
            JOIN Vendor v ON p.VendorId = v.Id
            WHERE p.ModelId = @modelId
            GROUP BY p.VendorId";

            return _db.Query<ModelAnalysisDto>(sql, new { modelId });
        }

        // 3. Purchase history
        public IEnumerable<PurchaseDto> GetPurchaseHistory(int modelId)
        {
            string sql = @"
            SELECT
                p.PurchaseDate Date,
                v.VendorName,
                e.EquipmentName,
                c.CategoryName,
                p.UnitPrice Price
            FROM PurchaseHistory p
            JOIN Vendor v ON p.VendorId = v.Id
            JOIN Equipment e ON p.EquipmentId = e.Id
            JOIN Model m ON p.ModelId = m.Id
            JOIN Category c ON m.CategoryId = c.Id
            WHERE p.ModelId = @modelId
            ORDER BY p.PurchaseDate DESC";

            return _db.Query<PurchaseDto>(sql, new { modelId });
        }
    }
}
    
