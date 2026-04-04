using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysisDto.ShareDto;
using WpfApp2.modelDTO.analysysDto;
 // Thư mục chứa AnalysisShareDto và MonthlySpendDto

namespace WpfApp2.Services.analysisService
{
    public class VendorAnalysisSv
    {
        private readonly DatabaseService _db = new DatabaseService();

        public VendorAnalysisDto GetVendorAnalysis(int vendorId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var conn = _db.GetConnection();

            string sql = @"
                -- 1. Lấy tổng quan (Summary)
                SELECT 
                    COUNT(p.Id) AS TotalTransactions,
                    IFNULL(SUM(p.Quantity * p.UnitPrice), 0) AS TotalPrice
                FROM PurchaseHistory p
                WHERE p.VendorId = @vendorId 
                AND (@fromDate IS NULL OR p.PurchaseDate >= @fromDate)
                AND (@toDate IS NULL OR p.PurchaseDate <= @toDate);

                -- 2. Lấy danh sách chi tiết (Details)
                SELECT 
                    p.Id, m.ModelName, m.ModelCode, b.BrandName, v.VendorName, 
                    e.EquipmentName, p.Quantity, p.UnitPrice, 
                    (p.Quantity * p.UnitPrice) AS LineTotal,
                    p.PurchaseDate, u.UserName AS FullName, p.Note
                FROM PurchaseHistory p
                INNER JOIN Model m ON p.ModelId = m.Id
                INNER JOIN Brand b ON m.BrandId = b.Id
                LEFT JOIN Vendor v ON p.VendorId = v.Id
                LEFT JOIN Equipment e ON p.EquipmentId = e.Id
                LEFT JOIN [User] u ON p.UserId = u.Id
                WHERE p.VendorId = @vendorId
                AND (@fromDate IS NULL OR p.PurchaseDate >= @fromDate)
                AND (@toDate IS NULL OR p.PurchaseDate <= @toDate)
                ORDER BY p.PurchaseDate DESC;";

            using var multi = conn.QueryMultiple(sql, new
            {
                vendorId,
                fromDate = fromDate?.ToString("yyyy-MM-dd"),
                toDate = toDate?.ToString("yyyy-MM-dd")
            });

            var result = multi.ReadFirst<VendorAnalysisDto>();
            var items = multi.Read<PurchaseDto>().ToList();
            result.Items = items;

            if (items.Any())
            {
                // Biểu đồ tròn: Group theo ModelCode và gán vào CategoryName
                result.ModelShares = items
                    .GroupBy(x => x.ModelCode)
                    .Select(g => new AnalysisShareDto
                    {
                        CategoryName = g.Key,
                        TotalAmount = g.Sum(x => x.LineTotal),
                        Percentage = (double)(g.Sum(x => x.LineTotal) / result.TotalPrice * 100)
                    })
                    .OrderByDescending(x => x.TotalAmount)
                    .Take(10) // Lấy Top 10 để biểu đồ không bị rối
                    .ToList();

                // Biểu đồ cột: Group theo Tháng
                result.MonthlySpends = items
                    .GroupBy(x => new { x.PurchaseDate.Year, x.PurchaseDate.Month })
                    .Select(g => new MonthlySpendDto
                    {
                        MonthYear = $"{g.Key.Month:D2}/{g.Key.Year}",
                        Amount = g.Sum(x => x.LineTotal)
                    })
                    .OrderBy(x => DateTime.ParseExact(x.MonthYear, "MM/yyyy", null))
                    .ToList();
            }
            return result;
        }
    }
}