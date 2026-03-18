using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO.analysysDto
{
    public class VendorAnalysisDto
    {
        public int TotalQuantity { get; set; }
        public decimal TotalPrice { get; set; }

        public int SupplyCount { get; set; }     // số lần cung cấp
        public int TotalEquipment { get; set; }  // số linh kiện khác nhau

        public List<PurchaseDto> Items { get; set; }
    }
}
