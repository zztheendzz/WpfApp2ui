using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO.analysisDto.ShareDto
{
    public class AnalysisShareDto
    {
        // 1. Nhãn hiển thị (Cái tên hiện lên trên mẩu bánh của biểu đồ)
        // Nó có thể là ModelCode, BrandName hoặc VendorName tùy vào Service gán vào.
        public string CategoryName { get; set; }

        // 2. Giá trị bằng tiền (Để tính độ lớn của mẩu bánh)
        public decimal TotalAmount { get; set; }

        // 3. Tỷ lệ phần trăm (Để hiển thị nhãn 25%, 30%...)
        public double Percentage { get; set; }

        // 4. (Tùy chọn) Tổng số lượng linh kiện
        public int TotalQuantity { get; set; }
    }
}
