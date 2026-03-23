using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;

namespace WpfApp2.Services.analysisService
{
    public class PurchaseAnalysisSv
    {
        public DatabaseService _db = new DatabaseService();
        // Equipment equipoment
        public IEnumerable<PurchaseDto> Search(
            string modelName,
            string vendor,
            string equipment,
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

            // ✅ Model Name
            if (!string.IsNullOrWhiteSpace(modelName))
            {
                sql.Append(" AND LOWER(m.ModelName) LIKE @modelName");
                param.Add("modelName", $"%{modelName.ToLower().Trim()}%");
            }

            // ✅ Vendor
            if (!string.IsNullOrWhiteSpace(vendor))
            {
                sql.Append(" AND LOWER(v.VendorName) LIKE @vendor");
                param.Add("vendor", $"%{vendor.ToLower().Trim()}%");
            }

            // ✅ Equipment (text search)
            if (!string.IsNullOrWhiteSpace(equipment))
            {
                sql.Append(" AND LOWER(e.EquipmentName) LIKE @equipment");
                param.Add("equipment", $"%{equipment.ToLower().Trim()}%");
            }

            // ✅ Dropdown filter
            if (equipmentId.HasValue)
            {
                sql.Append(" AND p.EquipmentId = @equipmentId");
                param.Add("equipmentId", equipmentId);
            }


            if (from.HasValue)
            {
                sql.Append(" AND p.PurchaseDate >= @from");
                param.Add("from", from);
            }

            if (to.HasValue)
            {
                sql.Append(" AND p.PurchaseDate <= @to");
                param.Add("to", to);
            }

            if (minPrice.HasValue)
            {
                sql.Append(" AND p.UnitPrice >= @minPrice");
                param.Add("minPrice", minPrice);
            }

            if (maxPrice.HasValue)
            {
                sql.Append(" AND p.UnitPrice <= @maxPrice");
                param.Add("maxPrice", maxPrice);
            }

            sql.Append(" ORDER BY p.PurchaseDate DESC");

            using var conn = _db.GetConnection();
            return conn.Query<PurchaseDto>(sql.ToString(), param);
        }

        public IEnumerable<PurchaseDto> Search3(
            int? EquipmentId,
            int? ModelId,
            int? VendorId,
            decimal? PriceMin,
            decimal? PriceMax
            )
        {
            string sql = @"
                SELECT 
                    p.*,
                    m.ModelName,
                    v.VendorName,
                    e.EquipmentName
                FROM PurchaseHistory p
                LEFT JOIN Model m ON p.ModelId = m.Id
                LEFT JOIN Vendor v ON p.VendorId = v.Id
                LEFT JOIN Equipment e ON p.EquipmentId = e.Id
                WHERE 1=1
                    AND (@ModelId IS NULL OR p.ModelId = @ModelId)
                    AND (@VendorId IS NULL OR p.VendorId = @VendorId)
                    AND (@EquipmentId IS NULL OR p.EquipmentId = @EquipmentId)
                    AND (@PriceMin IS NULL OR p.TotalPrice >= @PriceMin)
                    AND (@PriceMax IS NULL OR p.TotalPrice <= @PriceMax)
    ";

            using var conn = _db.GetConnection();

            var param = new
            {
                ModelId = ModelId,
                VendorId = VendorId,
                EquipmentId = EquipmentId,
                PriceMin=PriceMin,
                PriceMax=PriceMax
            };

            return conn.Query<PurchaseDto>(sql, param);
        }


    }
}