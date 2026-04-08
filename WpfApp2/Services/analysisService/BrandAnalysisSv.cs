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
    public class BrandAnalysisSv
    {
        private readonly DatabaseService _db = new DatabaseService();

        public BrandAnalysisDto GetBrandAnalysis(int brandId, DateTime? fromDate = null, DateTime? toDate = null)
        {
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
                AND (@toDate IS NULL OR p.PurchaseDate <= @toDate);

                -- 2. Detail List
                SELECT 
                    p.Id, m.ModelName, m.ModelCode, v.VendorName, e.EquipmentName,
                    c.CurrencyName AS CurrencyName, p.Quantity, p.UnitPrice,
                    (p.Quantity * p.UnitPrice) AS LineTotal,
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

            using var multi = conn.QueryMultiple(sql, new
            {
                brandId,
                fromDate = fromDate?.ToString("yyyy-MM-dd"),
                toDate = toDate?.ToString("yyyy-MM-dd")
            });

            var result = multi.ReadFirst<BrandAnalysisDto>();
            var items = multi.Read<PurchaseDto>().ToList();
            result.Items = items;

            // ===================== CHART =====================
            if (items.Any())
            {
                // ===== 1. GROUP FULL DATA =====
                var grouped = items
                    .GroupBy(x => x.ModelCode)
                    .Select(g => new AnalysisShareDto
                    {
                        CategoryName = g.Key,
                        TotalAmount = g.Sum(x => x.LineTotal)
                    })
                    .OrderByDescending(x => x.TotalAmount)
                    .ToList();

                // ===== 2. TOP 9 =====
                var top9 = grouped.Take(9).ToList();

                // ===== 3. OTHERS =====
                var othersAmount = grouped.Skip(9).Sum(x => x.TotalAmount);

                if (othersAmount > 0)
                {
                    top9.Add(new AnalysisShareDto
                    {
                        CategoryName = "Others",
                        TotalAmount = othersAmount
                    });
                }

                // ===== 4. TÍNH LẠI % (QUAN TRỌNG) =====
                foreach (var item in top9)
                {
                    item.Percentage = result.TotalPrice > 0
                        ? (double)item.TotalAmount / (double)result.TotalPrice * 100
                        : 0;
                }

                result.ModelShares = top9;

                // ===== 5. MONTHLY CHART =====
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
            else
            {
                result.ModelShares = new List<AnalysisShareDto>();
                result.MonthlySpends = new List<MonthlySpendDto>();
            }

            return result;
        }
    }
}