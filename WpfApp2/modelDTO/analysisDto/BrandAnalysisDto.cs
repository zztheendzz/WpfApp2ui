using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO.analysysDto
{
    public class BrandAnalysisDto
    {
        public string BrandName { get; set; }
        public int Id { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalPrice { get; set; }
        public List<PurchaseDto> Items { get; set; }
    }
}
