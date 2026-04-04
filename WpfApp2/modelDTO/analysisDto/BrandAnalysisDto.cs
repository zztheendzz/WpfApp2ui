using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDTO.analysisDto.ShareDto;

namespace WpfApp2.modelDTO.analysysDto
{
    public class BrandAnalysisDto
    {
        public decimal TotalPrice { get; set; }
        public int TotalTransactions { get; set; }
        public List<PurchaseDto> Items { get; set; }

        // Dữ liệu cho biểu đồ tròn (Model - Tỷ trọng)
        public List<AnalysisShareDto> ModelShares { get; set; }

        // Dữ liệu cho biểu đồ cột (Tháng - Giá trị)
        public List<MonthlySpendDto> MonthlySpends { get; set; }
    }




}

