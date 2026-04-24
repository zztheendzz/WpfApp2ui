using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;
using WpfApp2.modelDTO.analysisDto.ShareDto;
using System.Windows;

namespace WpfApp2.Services.analysisService
{
    public class PurchaseAnalysisSv
    {
        private readonly DatabaseService _db = new DatabaseService();

        public ModelAnalysisDto GetComprehensiveAnalysis(
            string modelSearch,
            int? modelId,
            int? vendorId,
            int? equipmentId,
            int? brandId,
            DateTime? from,
            DateTime? to,
            decimal? minPrice,
            decimal? maxPrice)
        {


            var sql = new StringBuilder(@"
                SELECT 
                    p.Id, 
                    m.ModelName, m.ModelCode,m.Image,
                    v.VendorName, 
                    e.EquipmentName, 
                    b.BrandName,
                    p.Quantity, 
                    p.UnitPrice, 
                    (p.Quantity * p.UnitPrice) AS TotalPrice,
                    c.CurrencyName,
                    p.PurchaseDate, 
                    u.FullName,
                    p.Note
                FROM PurchaseHistory p
                LEFT JOIN Model m ON p.ModelId = m.Id
                LEFT JOIN Vendor v ON p.VendorId = v.Id
                LEFT JOIN Equipment e ON p.EquipmentId = e.Id

                -- FIX CHỖ NÀY
                LEFT JOIN Brand b ON m.BrandId = b.Id

                LEFT JOIN Currency c ON p.CurrencyId = c.Id
                LEFT JOIN [User] u ON p.UserId = u.Id
    WHERE 1=1
            ");

            var param = new DynamicParameters();

            // Lọc theo ID (Dropdown) hoặc Text (Searchbox)
            if (modelId.HasValue)
            {
                sql.Append(" AND p.ModelId = @modelId");
                param.Add("modelId", modelId);
            }
            //if (!string.IsNullOrWhiteSpace(modelSearch))
            //{
            //    sql.Append(" AND (LOWER(m.ModelName) LIKE @ms OR LOWER(m.ModelCode) LIKE @ms)");
            //    param.Add("ms", $"%{modelSearch.ToLower()}%");
            //}

            if (vendorId.HasValue)
            {
                sql.Append(" AND p.VendorId = @vendorId");
                param.Add("vendorId", vendorId);
            }

            if (equipmentId.HasValue)
            {
                sql.Append(" AND p.EquipmentId = @equipmentId");
                param.Add("equipmentId", equipmentId);
            }
            if (brandId.HasValue)
            {
                sql.Append(" AND m.BrandId = @brandId");
                param.Add("brandId", brandId);
            }


            //Lọc theo khoảng thời gian
            if (from.HasValue)
            {
                sql.Append(" AND p.PurchaseDate >= @from");
                param.Add("from", from.Value.Date);
            }
            if (to.HasValue)
            {
                sql.Append(" AND p.PurchaseDate <= @to");
                param.Add("to", to.Value.Date.AddDays(1));
            }

            // Lọc theo đơn giá
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
            var items = conn.Query<PurchaseDto>(sql.ToString(), param).ToList();

            // Khởi tạo kết quả trả về
            var result = new ModelAnalysisDto { Items = items, TotalRecord = items.Count };

            if (items.Any())
            {
                // --- 1. TÍNH TOÁN KPI GIÁ ---
                result.MinPrice = items.Min(x => x.UnitPrice);
                result.MaxPrice = items.Max(x => x.UnitPrice);
                result.AvgPrice = items.Average(x => x.UnitPrice);
                result.GrandTotal = items.Sum(x => x.Quantity * x.UnitPrice);

                var lastPurchase = items.OrderByDescending(x => x.PurchaseDate).First();
                result.LastPrice = lastPurchase.UnitPrice;
                result.LastVendorName = lastPurchase.VendorName;

                // --- 2. DỮ LIỆU BIỂU ĐỒ CỘT (So sánh giá giữa các Vendor) ---
                // Lấy đơn giá trung bình mà mỗi Vendor đang bán cho Model này
                result.VendorComparison = items.GroupBy(x => x.VendorName)
                    .Select(g => new AnalysisShareDto
                    {
                        CategoryName = g.Key,
                        TotalAmount = g.Average(x => x.UnitPrice)
                    })
                    .OrderBy(x => x.TotalAmount)
                    .ToList();

                // --- 3. DỮ LIỆU BIỂU ĐỒ ĐƯỜNG (Biến động giá theo thời gian) ---
                result.PriceTrend = items.OrderBy(x => x.PurchaseDate)
                    .Select(x => new MonthlySpendDto
                    {
                        MonthYear = x.PurchaseDate.ToString("dd/MM/yy"),
                        Amount = x.UnitPrice
                    }).ToList();
            }

            return result;
        }
    }
}