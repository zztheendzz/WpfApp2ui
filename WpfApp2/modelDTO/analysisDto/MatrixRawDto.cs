using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO.analysisDto
{
    public class MatrixRawDto
    {
        public string ModelName { get; set; }
        public string VendorName { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}
