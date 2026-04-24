using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO.analysisDto
{
    public class ModelVendorMatrixRowDto
    {
        public string ModelName { get; set; }
        public string ModelCode { get; set; }
        // key = VendorName, value = latest price
        public Dictionary<string, decimal?> VendorPrices { get; set; } = new();
        public string Image { get; set; }
        public bool IsTotalRow { get; set; } // để phân biệt dòng TOTAL
    }
}
