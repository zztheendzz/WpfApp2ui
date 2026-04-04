using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDTO.analysisDto.ShareDto;

namespace WpfApp2.modelDTO.analysysDto
{
    public class EquipmentAnalysisDto
    {
        public string EquipmentName { get; set; }

        // --- 2. Số liệu tổng quát (Hiển thị trên 3 Card trên cùng) ---
        public decimal TotalPrice { get; set; }        // Tổng giá trị (VND)
        public int TotalTransactions { get; set; }   // Số lần mua/thay thế
        public int TotalModel { get; set; }          // Số lượng loại linh kiện khác nhau

        // --- 3. Dữ liệu cho Biểu đồ (Sử dụng AnalysisShareDto của bạn) ---

        // Dùng cho biểu đồ Tròn: Phân bổ chi phí theo Thương hiệu (Brand)
        public List<AnalysisShareDto> BrandShares { get; set; }

        // Dùng cho biểu đồ Cột ngang: Top 5 linh kiện "ngốn" nhiều tiền nhất
        public List<AnalysisShareDto> TopItems { get; set; }

        // Dùng cho biểu đồ Cột dọc (nếu cần): Biến động chi tiêu theo tháng
        public List<MonthlySpendDto> MonthlySpends { get; set; }

        // --- 4. Danh sách chi tiết (Hiển thị trên DataGrid) ---
        public List<PurchaseDto> Items { get; set; }

        public EquipmentAnalysisDto()
        {
            BrandShares = new List<AnalysisShareDto>();
            TopItems = new List<AnalysisShareDto>();
            MonthlySpends = new List<MonthlySpendDto>();
            Items = new List<PurchaseDto>();
        }
    }
}
