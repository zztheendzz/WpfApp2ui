using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO.analysysDto
{
    public class BrandAnalysisDto
    {
        public decimal TotalPrice { get; set; }
        public int TotalTransactions { get; set; }
        public List<PurchaseDto> Items { get; set; }

        // Dữ liệu cho biểu đồ tròn (Model - Tỷ trọng)
        public List<ModelShareDto> ModelShares { get; set; }

        // Dữ liệu cho biểu đồ cột (Tháng - Giá trị)
        public List<MonthlySpendDto> MonthlySpends { get; set; }
    }

    public class ModelShareDto
    {
       
        public string ModelCode { get; set; }
        public decimal TotalAmount { get; set; }
        public double Percentage { get; set; }
    }

    public class MonthlySpendDto
    {
        public string MonthYear { get; set; } // Ví dụ: "01/2026"
        public decimal Amount { get; set; }
    }
}

