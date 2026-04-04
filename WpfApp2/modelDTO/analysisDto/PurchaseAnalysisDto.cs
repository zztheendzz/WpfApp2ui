using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO.analysisDto
{
    public class PurchaseAnalysisDto
    {

        public List<PurchaseDto> Items { get; set; }
        public decimal LineTotal { get; set; }
    }
}
