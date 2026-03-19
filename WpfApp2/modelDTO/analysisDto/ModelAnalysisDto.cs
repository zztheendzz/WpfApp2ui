using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDTO.analysisDto;

namespace WpfApp2.modelDTO.analysysDto
{
    public class ModelAnalysisDto
    {
        public int ModelId { get; set; }
        public string ModelName { get; set; }

        // Giá
        public double LatestPrice { get; set; }
        public double MinPrice { get; set; }
        public double MaxPrice { get; set; }
        public double AvgPrice { get; set; }

        // Vendor tốt nhất (giá thấp nhất)
        public int BestVendorId { get; set; }
        public string BestVendorName { get; set; }
        public double BestVendorPrice { get; set; }
        public List<VendorPriceDto> Vendors { get; set; }

        public List<PurchaseDto> Items { get; set; }
    }
}
