using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO.analysisDto.ShareDto
{
    public class MonthlySpendDto
    {
        public string MonthYear { get; set; } // Định dạng "MM/yyyy"
        public decimal Amount { get; set; }
    }
}
