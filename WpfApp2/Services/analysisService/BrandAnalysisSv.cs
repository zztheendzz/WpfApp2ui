using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks; // Cần thêm namespace này
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysisDto.ShareDto;
using WpfApp2.modelDTO.analysysDto;

namespace WpfApp2.Services.analysisService
{
    public class BrandAnalysisSv
    {
        private readonly DatabaseService _db = new DatabaseService();

        // Chuyển sang Task<T> và thêm hậu tố Async
        public async Task<BrandAnalysisDto> GetBrandAnalysisAsync(int brandId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Sử dụng await để mở kết nối bất đồng bộ (tùy thuộc vào DatabaseService của bạn)
            using var conn = _db.GetConnection();

            string sql = @"
                -- 1. Summary
                SELECT 
                    COUNT(p.Id) AS TotalTransactions,
                    IFNULL(SUM(p.Quantity * p.UnitPrice), 0) AS TotalPrice
                FROM PurchaseHistory p
                JOIN Model m ON p.ModelId = m.Id
                WHERE m.BrandId = @brandId 
                AND (@fromDate IS NULL OR p.PurchaseDate >= @fromDate)
                AND (@toDate IS NULL OR p.PurchaseDate < @toDate);

                -- 2. Detail List
                SELECT 
                    p.Id, m.ModelName, m.ModelCode,m.Image, v.VendorName, e.EquipmentName,
                    c.CurrencyName AS CurrencyName, p.Quantity, p.UnitPrice,
                    (p.Quantity * p.UnitPrice) AS LineTotal,
                    (p.Quantity * p.UnitPrice) AS TotalPrice,
                    p.PurchaseDate, u.UserName AS FullName, p.Note
                FROM PurchaseHistory p
                INNER JOIN Model m ON p.ModelId = m.Id
                LEFT JOIN Vendor v ON p.VendorId = v.Id
                LEFT JOIN Currency c ON p.CurrencyId = c.Id
                LEFT JOIN Equipment e ON p.EquipmentId = e.Id
                LEFT JOIN [User] u ON p.UserId = u.Id
                WHERE m.BrandId = @brandId
                AND (@fromDate IS NULL OR p.PurchaseDate >= @fromDate)
                AND (@toDate IS NULL OR p.PurchaseDate <= @toDate)
                ORDER BY p.PurchaseDate DESC;";

            // Dùng QueryMultipleAsync thay cho QueryMultiple
            using var multi = await conn.QueryMultipleAsync(sql, new
            {
                brandId,
                fromDate = fromDate?.Date,
                toDate = toDate?.Date.AddDays(1)
            });

            // Đọc dữ liệu bất đồng bộ
            var result = await multi.ReadFirstAsync<BrandAnalysisDto>();
            var items = (await multi.ReadAsync<PurchaseDto>()).ToList();
            result.Items = items;

            // Phần tính toán LINQ có thể giữ nguyên vì nó thao tác trên Memory.
            // Tuy nhiên, nếu items có hàng chục ngàn dòng, hãy cân nhắc bọc trong Task.Run
            if (items.Any())
            {
                // Logic Chart (giữ nguyên logic của bạn)
                var grouped = items
                    .GroupBy(x => x.ModelCode)
                    .Select(g => new AnalysisShareDto
                    {
                        CategoryName = g.Key,
                        TotalAmount = g.Sum(x => x.TotalPrice),
                    })
                    .OrderByDescending(x => x.TotalAmount)
                    .ToList();

                var top9 = grouped.Take(9).ToList();
                var othersAmount = grouped.Skip(9).Sum(x => x.TotalAmount);

                if (othersAmount > 0)
                {
                    top9.Add(new AnalysisShareDto
                    {
                        CategoryName = "Others",
                        TotalAmount = othersAmount
                    });
                }

                foreach (var item in top9)
                {
                    item.Percentage = result.TotalPrice > 0
                        ? (double)item.TotalAmount / (double)result.TotalPrice * 100
                        : 0;
                }

                result.ModelShares = top9;

                result.MonthlySpends = items
                    .GroupBy(x => new { x.PurchaseDate.Year, x.PurchaseDate.Month })
                    .Select(g => new MonthlySpendDto
                    {
                        MonthYear = $"{g.Key.Month:D2}/{g.Key.Year}",
                        Amount = g.Sum(x => x.TotalPrice),
                    })
                    .OrderBy(x => DateTime.ParseExact(x.MonthYear, "MM/yyyy", null))
                    .ToList();
            }
            else
            {
                result.ModelShares = new List<AnalysisShareDto>();
                result.MonthlySpends = new List<MonthlySpendDto>();
            }

            return result;
        }
    }
}