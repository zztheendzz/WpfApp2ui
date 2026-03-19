using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.model.modelImportExcel
{
    public class ImportPurchase
    {
        public int No { get; set; }
        public string ModelName { get; set; }
        public string Brand { get; set; }
        public string ModelCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Vendor { get; set; }
        public string Note { get; set; }

    }
}
