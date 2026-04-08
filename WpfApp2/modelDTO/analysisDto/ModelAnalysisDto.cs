using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDTO.analysisDto;
using WpfApp2.modelDTO.analysisDto.ShareDto;

namespace WpfApp2.modelDTO.analysysDto
{
    public class ModelAnalysisDto
    {

        public decimal LastPrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal AvgPrice { get; set; }
        public int TotalRecord { get; set; }
        public string LastVendorName { get; set; }
        public List<PurchaseDto> Items { get; set; } = new();
        public List<AnalysisShareDto> VendorComparison { get; set; } = new();
        public List<MonthlySpendDto> PriceTrend { get; set; } = new();
        public int ModelId { get; set; }
        public string ModelName { get; set; }
        public string ModelCode{ get; set; }
        public int CategoryId { get; set; }
        public int CurrentcyId { get; set; }
        public string CurrencyCode { get; set; }
        public string FullName { get; set; }
        public string CurrencyName { get; set; }
        // Giá
        public double LatestPrice { get; set; }

        // Vendor tốt nhất (giá thấp nhất)
        public int BestVendorId { get; set; }
        public string BestVendorName { get; set; }
        public double BestVendorPrice { get; set; }
        public List<VendorPriceDto> Vendors { get; set; }

    }
}
