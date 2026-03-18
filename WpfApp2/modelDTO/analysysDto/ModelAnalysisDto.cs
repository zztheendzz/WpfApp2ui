using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO.analysysDto
{
    public class ModelAnalysisDto
    {
        public decimal Entity { get; set; }
        public decimal TotalPrice { get; set; }
        public List<PurchaseDto> purchaseDtos { get; set; }
    }
}
