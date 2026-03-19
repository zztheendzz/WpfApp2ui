using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO.analysisDto
{
    public class VendorPriceDto
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }

        public double MinPrice { get; set; }
        public double MaxPrice { get; set; }
        public double AvgPrice { get; set; }
    }
}
