using ClosedXML.Excel;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WpfApp2.modelDTO.analysisDto;
using WpfApp2.modelDTO;

namespace WpfApp2.Services.exportExcel
{
    public class ExportSv
    {
        public void ExportModelMatrix(ModelVendorMatrixDto matrixData, string outputPath)
        {
            // 1. Kiểm tra đầu vào tổng quát
            if (matrixData == null || matrixData.Rows == null || matrixData.Vendors == null)
                throw new Exception("Dữ liệu không hợp lệ hoặc bị trống.");

            string templatePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "excelTemplate",
                "template.xlsx"
            );

            if (!File.Exists(templatePath))
                throw new Exception("Không tìm thấy file template.xlsx");

            using (var workbook = new XLWorkbook(templatePath))
            {
                var ws = workbook.Worksheet(1);
                int startRow = 4;
                int stt = 1;

                // --- Bước 1: Điền tên Vendor vào Header (Dòng 2) ---
                int headerCol = 7;
                foreach (var vName in matrixData.Vendors)
                {
                    if (headerCol > 14) break;
                    ws.Cell(2, headerCol).Value = vName ?? "Unknown Vendor";
                    headerCol += 2;
                }

                // --- Bước 2: Điền dữ liệu từng dòng ---
                foreach (var row in matrixData.Rows)
                {
                    if (row == null) continue;

                    ws.Cell(startRow, 1).Value = stt++;
                    ws.Cell(startRow, 2).Value = row.ModelName ?? "";
                    ws.Cell(startRow, 3).Value = row.ModelCode ?? "";
                    ws.Cell(startRow, 6).Value = 1; // Số lượng mẫu

                    int vendorColIndex = 7;
                    foreach (var vendorName in matrixData.Vendors)
                    {
                        if (vendorColIndex > 14) break;

                        // KIỂM TRA THIẾU DATA Ở ĐÂY
                        if (row.VendorPrices != null && row.VendorPrices.ContainsKey(vendorName))
                        {
                            var priceVal = row.VendorPrices[vendorName];

                            // Nếu có giá trị và không phải null
                            if (priceVal != null && decimal.TryParse(priceVal.ToString(), out decimal price))
                            {
                                ws.Cell(startRow, vendorColIndex).Value = price;
                                // Công thức tính tiền
                                ws.Cell(startRow, vendorColIndex + 1).FormulaA1 = $"=F{startRow}*{ws.Cell(startRow, vendorColIndex).Address}";
                            }
                            else
                            {
                                // Nếu thiếu giá (như ô trống trong ảnh), để trống hoặc ghi 0
                                ws.Cell(startRow, vendorColIndex).Value = "";
                                ws.Cell(startRow, vendorColIndex + 1).Value = "";
                            }
                        }

                        vendorColIndex += 2;
                    }

                    // Kẻ khung cho đẹp
                    var range = ws.Range(startRow, 1, startRow, 16);
                    range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Nếu là dòng TOTAL thì cho in đậm
                    if (row.IsTotalRow)
                    {
                        range.Style.Font.Bold = true;
                        ws.Cell(startRow, 1).Value = ""; // Dòng Total không cần STT
                    }

                    startRow++;
                }

                workbook.SaveAs(outputPath);
            }
        }

        public async Task ExportPurchaseHistoryAsync(List<PurchaseDto> purchaseData, string outputPath)
        {
            if (purchaseData == null)
                throw new Exception("Dữ liệu không hợp lệ hoặc bị trống.");

            string templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "excelTemplate",
                "purchase_history_template.xlsx"
            );

            if (!File.Exists(templatePath))
                throw new Exception("Không tìm thấy file purchase_history_template.xlsx");

            await Task.Run(() =>
            {
                using (var workbook = new XLWorkbook(templatePath))
                {
                    var ws = workbook.Worksheet(1);
                    int startRow = 5;
                    int i = 0;

                    foreach (var item in purchaseData)
                    {
                        i++;

                        ws.Cell(startRow, 1).Value = i;
                        ws.Cell(startRow, 2).Value = item.ModelName ?? "";
                        ws.Cell(startRow, 3).Value = item.BrandName ?? "";
                        ws.Cell(startRow, 4).Value = item.ModelCode ?? "";

                        ws.Cell(startRow, 6).Value = item.Quantity;
                        ws.Cell(startRow, 6).Style.NumberFormat.Format = "#,##0";

                        ws.Cell(startRow, 7).Value = item.UnitPrice;
                        ws.Cell(startRow, 7).Style.NumberFormat.Format = "#,##0";

                        ws.Cell(startRow, 8).Value = item.TotalPrice;
                        ws.Cell(startRow, 8).Style.NumberFormat.Format = "#,##0";

                        ws.Cell(startRow, 9).Value = item.Note ?? "";

                        var range = ws.Range(startRow, 1, startRow, 13);
                        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        startRow++;
                    }
                    int totalRow = startRow;

                    // Ghi chữ "Tổng"
                    ws.Cell(totalRow, 7).Value = "Sum:";
                    ws.Cell(totalRow, 7).Style.Font.Bold = true;

                    // Công thức SUM cột TotalPrice (cột 8)
                    ws.Cell(totalRow, 8).FormulaA1 = $"SUM(H5:H{startRow - 1})";
                    ws.Cell(totalRow, 8).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(totalRow, 8).Style.Font.Bold = true;

                    // Kẻ khung
                    var totalRange = ws.Range(totalRow, 7, totalRow, 8);
                    totalRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    workbook.SaveAs(outputPath);
                }
            });
        }


        public void ExportPurchaseHistory(List<PurchaseDto> purchaseData, string outputPath)
        {
            if (purchaseData == null)
                throw new Exception("Dữ liệu không hợp lệ hoặc bị trống.");
            string templatePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "excelTemplate",
                "purchase_history_template.xlsx"
            );
            if (!File.Exists(templatePath))
                throw new Exception("Không tìm thấy file purchase_history_template.xlsx");
            using (var workbook = new XLWorkbook(templatePath))
            {
                var ws = workbook.Worksheet(1);
                int startRow = 5;
                int i = 0;
                foreach (var item in purchaseData)
                {
                    i++ ;

                    ws.Cell(startRow, 1).Value = i;
                    ws.Cell(startRow, 2).Value = item.ModelName ?? "";
                    ws.Cell(startRow, 3).Value = item.BrandName ?? "";
                    ws.Cell(startRow, 4).Value = item.ModelCode ?? "";
                    ws.Cell(startRow, 6).Value = item.Quantity;
                    ws.Cell(startRow, 6).Style.NumberFormat.Format = "#,##0";

                    ws.Cell(startRow, 7).Value = item.UnitPrice;
                    ws.Cell(startRow, 7).Style.NumberFormat.Format = "#,##0";

                    ws.Cell(startRow, 8).Value = item.TotalPrice;
                    ws.Cell(startRow, 8).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(startRow, 9).Value = item.Note ?? "";
                    // Kẻ khung cho đẹp
                    var range = ws.Range(startRow, 1, startRow, 13);
                    range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    startRow++;
                }
                workbook.SaveAs(outputPath);
            }
        }
    }
}