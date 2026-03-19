using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO.analysysDto
{
    public class EquipmentAnalysisDto
    {
        public int TotalModel { get; set; }
        public decimal TotalPrice { get; set; }

        public List<PurchaseDto> Items { get; set; }
    }
}
