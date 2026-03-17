using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO.analysysDto
{
    public class ModelAnalysisDto
    {
        public string ModelName { get; set; }

        public string VendorName { get; set; }

        public decimal LastPrice { get; set; }

        public decimal MinPrice { get; set; }

        public decimal MaxPrice { get; set; }

        public decimal AvgPrice { get; set; }

        public DateTime? LastPurchaseDate { get; set; }
    }
}
