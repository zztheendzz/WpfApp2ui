using System;
using System.Collections.Generic;
using WpfApp2.modelDTO.analysisDto.ShareDto; // Đảm bảo đúng namespace chứa AnalysisShareDto

namespace WpfApp2.modelDTO.analysysDto
{
    public class VendorAnalysisDto
    {
        // --- 1. KPI Cards ---
        public decimal TotalPrice { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalQuantity { get; set; }

        // --- 2. Danh sách chi tiết (DataGrid) ---
        public List<PurchaseDto> Items { get; set; } = new List<PurchaseDto>();

        // --- 3. Dữ liệu Biểu đồ (Sử dụng chung AnalysisShareDto) ---

        // Tỷ lệ chi tiêu theo từng Model (Mục tiêu: ModelCode -> CategoryName)
        public List<AnalysisShareDto> ModelShares { get; set; } = new List<AnalysisShareDto>();

        // Tỷ lệ chi tiêu theo Thương hiệu (Mục tiêu: BrandName -> CategoryName)
        public List<AnalysisShareDto> BrandShares { get; set; } = new List<AnalysisShareDto>();

        // Biến động chi tiêu theo tháng
        public List<MonthlySpendDto> MonthlySpends { get; set; } = new List<MonthlySpendDto>();
    }
}