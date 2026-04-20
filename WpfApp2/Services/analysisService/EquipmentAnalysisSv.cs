using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

            // Chuyển đổi ngày sang string để SQLite xử lý chính xác
          //  string fromDateStr = fromDate?.ToString("yyyy-MM-dd");
         //   string toDateStr = toDate?.ToString("yyyy-MM-dd");

            string sql = @"
                -- 1. Summary & Equipment Name
                SELECT 
                    e.EquipmentName,
                    COUNT(p.Id) AS TotalTransactions,
                    IFNULL(SUM(p.Quantity * p.UnitPrice), 0) AS TotalPrice,
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
                    p.Id, m.ModelName, m.ModelCode, v.VendorName, b.BrandName,
                    c.CurrencyName, p.Quantity, p.UnitPrice,
                    (p.Quantity * p.UnitPrice) AS TotalPrice,
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

            // Sử dụng ReadFirstOrDefault để tránh crash nếu ID không tồn tại
            var result = multi.ReadFirstOrDefault<EquipmentAnalysisDto>();

            if (result == null) return new EquipmentAnalysisDto { EquipmentName = "N/A" };

            var items = multi.Read<PurchaseDto>().ToList();
            result.Items = items;

            if (items.Any())
            {
                // 1. Phân bổ chi phí theo Thương hiệu
                result.BrandShares = items
                    .GroupBy(x => x.BrandName ?? "Khác")
                    .Select(g => new AnalysisShareDto
                    {
                        CategoryName = g.Key,
                        TotalAmount = g.Sum(x => x.LineTotal),
                        Percentage = result.TotalPrice > 0
                            ? (double)(g.Sum(x => x.LineTotal) / result.TotalPrice * 100)
                            : 0
                    })
                    .OrderByDescending(x => x.TotalAmount)
                    .ToList();

                // 2. Top 5 Linh kiện có tổng giá trị cao nhất
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

                // 3. Biến động chi phí theo tháng (Định dạng: MM/yyyy)
                result.MonthlySpends = items
                    .GroupBy(x => new { x.PurchaseDate.Year, x.PurchaseDate.Month })
                    .Select(g => new MonthlySpendDto
                    {
                        MonthYear = $"{g.Key.Month:D2}/{g.Key.Year}",
                        Amount = g.Sum(x => x.LineTotal)
                    })
                    // Sắp xếp theo thời gian thực thay vì string
                    .OrderBy(x => {
                        var parts = x.MonthYear.Split('/');
                        return new DateTime(int.Parse(parts[1]), int.Parse(parts[0]), 1);
                    })
                    .ToList();
            }
            else
            {
                // Khởi tạo list trống thay vì để null để tránh lỗi Binding WPF
                result.BrandShares = new List<AnalysisShareDto>();
                result.TopItems = new List<AnalysisShareDto>();
                result.MonthlySpends = new List<MonthlySpendDto>();
            }

            return result;
        }
    }
}