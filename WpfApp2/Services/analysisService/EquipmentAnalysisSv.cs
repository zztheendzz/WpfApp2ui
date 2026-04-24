using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows; // Thêm thư viện này để dùng MessageBox
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysisDto.ShareDto;
using WpfApp2.modelDTO.analysysDto;

namespace WpfApp2.Services.analysisService
{
    public class EquipmentAnalysisSv
    {
        private readonly DatabaseService _db = new DatabaseService();

        public EquipmentAnalysisDto GetEquipmentAnalysis(int equipmentId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var conn = _db.GetConnection();

            string sql = @"
        -- 1. Summary
        SELECT 
            e.EquipmentName,
            COUNT(p.Id) AS TotalTransactions,
            CAST(IFNULL(SUM(p.Quantity * p.UnitPrice), 0) AS DECIMAL) AS TotalPrice, -- Ép kiểu ngay trong SQL
            (SELECT COUNT(DISTINCT p2.ModelId) 
             FROM PurchaseHistory p2 
             WHERE p2.EquipmentId = @equipmentId
             AND (@fromDate IS NULL OR p2.PurchaseDate >= @fromDate)
             AND (@toDate IS NULL OR p2.PurchaseDate <= @toDate)) AS TotalModel
        FROM Equipment e
        LEFT JOIN PurchaseHistory p ON e.Id = p.EquipmentId
            AND (@fromDate IS NULL OR p.PurchaseDate >= @fromDate)
            AND (@toDate IS NULL OR p.PurchaseDate <= @toDate)
        WHERE e.Id = @equipmentId
        GROUP BY e.Id, e.EquipmentName;

        -- 2. Detail List
        SELECT 
            p.Id, m.ModelName, m.ModelCode,m.Image, v.VendorName, b.BrandName,
            c.CurrencyName, 
            p.Quantity, 
            p.UnitPrice,
            CAST((p.Quantity * p.UnitPrice) AS DECIMAL) AS LineTotal, -- Đảm bảo tên cột là LineTotal
            CAST((p.Quantity * p.UnitPrice) AS DECIMAL) AS TotalPrice, -- Đổ vào cả 2 cho chắc chắn
            p.PurchaseDate, u.UserName AS FullName, p.Note
        FROM PurchaseHistory p
        INNER JOIN Model m ON p.ModelId = m.Id
        LEFT JOIN Brand b ON m.BrandId = b.Id
        LEFT JOIN Vendor v ON p.VendorId = v.Id
        LEFT JOIN Currency c ON p.CurrencyId = c.Id
        LEFT JOIN [User] u ON p.UserId = u.Id
        WHERE p.EquipmentId = @equipmentId
        AND (@fromDate IS NULL OR p.PurchaseDate >= @fromDate)
        AND (@toDate IS NULL OR p.PurchaseDate <= @toDate)
        ORDER BY p.PurchaseDate DESC;";

            using var multi = conn.QueryMultiple(sql, new
            {
                equipmentId,
                fromDate = fromDate?.Date,
                toDate = toDate?.Date.AddDays(1)
            });

            var result = multi.ReadFirstOrDefault<EquipmentAnalysisDto>();
            if (result == null) return new EquipmentAnalysisDto { EquipmentName = "N/A" };

            var items = multi.Read<PurchaseDto>().ToList();
            result.Items = items;

            // --- MESS 1: KIỂM TRA DỮ LIỆU GỐC SAU KHI QUERY ---
            if (items.Any())
            {
                var test = items.First();
            }
            else
            {
            }

            if (items.Any())
            {
                // 1. Phân bổ chi phí theo Thương hiệu
                result.BrandShares = items
                    .GroupBy(x => x.BrandName ?? "Khác")
                    .Select(g => {
                        var sum = g.Sum(x => x.LineTotal); // Tính tổng dòng
                        return new AnalysisShareDto
                        {
                            CategoryName = g.Key,
                            TotalAmount = sum,
                            Percentage = result.TotalPrice > 0 ? (double)(sum / result.TotalPrice * 100) : 0
                        };
                    })
                    .OrderByDescending(x => x.TotalAmount)
                    .ToList();

                // 2. Top 5 Linh kiện
                result.TopItems = items
                    .GroupBy(x => x.ModelName)
                    .Select(g => new AnalysisShareDto
                    {
                        CategoryName = g.Key,
                        TotalAmount = g.Sum(x => x.LineTotal)
                    })
                    .OrderByDescending(x => x.TotalAmount)
                    .Take(5)
                    .ToList();

                // 3. Biến động theo tháng
                result.MonthlySpends = items
                    .GroupBy(x => new { x.PurchaseDate.Year, x.PurchaseDate.Month })
                    .Select(g => new MonthlySpendDto
                    {
                        MonthYear = $"{g.Key.Month:D2}/{g.Key.Year}",
                        Amount = g.Sum(x => x.LineTotal)
                    })
                    .ToList();

                // --- MESS 2: KIỂM TRA DỮ LIỆU SAU KHI GROUP ---
                string detail = "--- DATA SAU KHI GROUP ---\n";
                foreach (var brand in result.BrandShares)
                {
                    detail += $"Brand: {brand.CategoryName} | Tiền: {brand.TotalAmount}\n";
                }
            }

            return result;
        }
    }
}