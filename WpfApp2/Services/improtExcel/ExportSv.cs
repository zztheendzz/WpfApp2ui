using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDTO.analysisDto;

namespace WpfApp2.Services.improtExcel
{
    public class ExportSv
    {
        public void ExportModelMatrix(ModelVendorMatrixDto matrix, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Matrix Analysis");

                // 1. TẠO HEADER
                worksheet.Cell(1, 1).Value = "Model";
                int colIndex = 2;
                foreach (var vendor in matrix.Vendors)
                {
                    worksheet.Cell(1, colIndex++).Value = vendor;
                }

                // Style cho Header (Bôi đậm, nền xám, căn giữa)
                var headerRange = worksheet.Range(1, 1, 1, colIndex - 1);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // 2. ĐỔ DỮ LIỆU DÒNG
                int rowIndex = 2;
                foreach (var row in matrix.Rows)
                {
                    // Cột Model Name
                    worksheet.Cell(rowIndex, 1).Value = row.ModelName;

                    // Các cột giá Vendor
                    int vColIndex = 2;
                    foreach (var vendor in matrix.Vendors)
                    {
                        if (row.VendorPrices.ContainsKey(vendor) && row.VendorPrices[vendor] != null)
                        {
                            var cell = worksheet.Cell(rowIndex, vColIndex);
                            cell.Value = (double)row.VendorPrices[vendor];
                            cell.Style.NumberFormat.Format = "#,##0"; // Định dạng số có dấu phẩy
                        }
                        vColIndex++;
                    }

                    // Nếu là dòng TOTAL thì bôi đậm và đổi màu nền
                    if (row.IsTotalRow || row.ModelName == "TOTAL")
                    {
                        var totalRange = worksheet.Range(rowIndex, 1, rowIndex, colIndex - 1);
                        totalRange.Style.Font.Bold = true;
                        totalRange.Style.Fill.BackgroundColor = XLColor.LightCyan;
                    }

                    rowIndex++;
                }

                // 3. ĐỊNH DẠNG CUỐI CÙNG
                // Kẻ ô cho toàn bộ bảng dữ liệu
                var dataRange = worksheet.Range(1, 1, rowIndex - 1, colIndex - 1);
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // Tự động căn chỉnh độ rộng cột theo nội dung
                worksheet.Columns().AdjustToContents();

                // Lưu file
                workbook.SaveAs(filePath);
            }
        }
    }
}
