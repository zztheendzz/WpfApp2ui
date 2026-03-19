
using ClosedXML.Excel;
using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model.modelImportExcel;
namespace WpfApp2.Services.improtExcel
{
    public class PurchaseExcelSv : INotifyPropertyChanged
    {
        private DatabaseService _db = new DatabaseService();

        private string vendorDict ="SELECT Id, VendorName AS Name FROM Vendor";


        public void inSertData(string filePath) {

            var importPurchases = ReadExcel(filePath);

            // load lookup 1 lần
            var vendorDict = GetDictionary("SELECT Id, VendorName AS Name FROM Vendor");
            var brandDict = GetDictionary("SELECT Id, BrandName AS Name FROM Brand");
            var modelDict = GetDictionary("SELECT Id, ModelName AS Name FROM Model");

            using var conn = _db.GetConnection();

            var errors = new List<string>();

            foreach (var row in importPurchases)
            {
                // ====== CLEAN DATA ======
                var modelName = row.ModelName?.Trim();
                var vendorName = row.Vendor?.Trim();
                var brandName = row.Brand?.Trim();

                // ====== VALIDATE ======
                if (string.IsNullOrEmpty(modelName))
                {
                    errors.Add($"Model empty");
                    continue;
                }

                if (!vendorDict.TryGetValue(vendorName, out int vendorId))
                {
                    errors.Add($"Vendor không tồn tại: {vendorName}");
                    continue;
                }

                if (!brandDict.TryGetValue(brandName, out int brandId))
                {
                    errors.Add($"Brand không tồn tại: {brandName}");
                    continue;
                }

                // model có thể cho phép auto create hoặc check
                if (!modelDict.TryGetValue(modelName, out int modelId))
                {
                    // 👉 option 1: báo lỗi
                    errors.Add($"Model không tồn tại: {modelName}");
                    continue;

                    // 👉 option 2: auto insert (nếu muốn)
                    
                    modelId = conn.ExecuteScalar<int>(
                        @"INSERT INTO Model(ModelName, VendorId, BrandId)
                          VALUES(@Name, @VendorId, @BrandId);
                          SELECT CAST(SCOPE_IDENTITY() as int);",
                        new { Name = modelName, VendorId = vendorId, BrandId = brandId }
                    );

                    modelDict[modelName] = modelId;
                    
                }

                // ====== INSERT PURCHASE ======
                conn.Execute(@"
        INSERT INTO Purchase(ModelId, VendorId, BrandId, Price)
        VALUES(@ModelId, @VendorId, @BrandId, @Price)
    ",
                new
                {
                    ModelId = modelId,
                    VendorId = vendorId,
                    BrandId = brandId,
                });
            }

        }



        public Dictionary<string, int> GetDictionary(string sql)
        {
            using var conn = _db.GetConnection();

            return conn.Query<(int Id, string Name)>(sql)
                .ToDictionary(
                    x => x.Name.Trim(),
                    x => x.Id,
                    StringComparer.OrdinalIgnoreCase
                );
        }


        public List<ImportPurchase> ReadExcel(string filePath)
        {
            var result = new List<ImportPurchase>();

            using var workbook = new XLWorkbook(filePath);
            var ws = workbook.Worksheet(1);

            // 1. Tìm dòng header (dòng chứa "STT")
            int headerRow = 0;

            foreach (var row in ws.RowsUsed())
            {
                if (row.Cells().Any(c => c.GetString().Trim().ToUpper() == "STT"))
                {
                    headerRow = row.RowNumber();
                    break;
                }
            }

            if (headerRow == 0)
                throw new Exception("Không tìm thấy header");

            // 2. Map cột theo tên
            var headerMap = new Dictionary<string, int>();

            var header = ws.Row(headerRow);

            foreach (var cell in header.Cells())
            {
                var value = cell.GetString().Trim();

                if (!string.IsNullOrEmpty(value))
                {
                    headerMap[value] = cell.Address.ColumnNumber;
                }
            }

            // 3. Đọc data từ dòng tiếp theo
            int currentRow = headerRow + 1;

            while (!ws.Row(currentRow).IsEmpty())
            {
                var row = ws.Row(currentRow);

                var item = new ImportPurchase
                {
                    //STT = GetInt(row, headerMap, "STT"),
                    //TenHang = GetString(row, headerMap, "Tên hàng"),
                    //NhanHieu = GetString(row, headerMap, "Nhãn hiệu"),
                    //MaHang = GetString(row, headerMap, "Mã hàng"),
                    //SoLuong = GetInt(row, headerMap, "Số lượng cần"),
                    //DonGia = GetDecimal(row, headerMap, "Đơn giá"),
                    //NhaCungUng = GetString(row, headerMap, "Nhà cung ứng")
                };

                result.Add(item);
                currentRow++;
            }

            return result;
        }



























        private string GetString(IXLRow row, Dictionary<string, int> map, string colName)
        {
            return map.ContainsKey(colName)
                ? row.Cell(map[colName]).GetString()
                : "";
        }

        private int GetInt(IXLRow row, Dictionary<string, int> map, string colName)
        {
            return map.ContainsKey(colName)
                ? row.Cell(map[colName]).GetValue<int>()
                : 0;
        }

        private decimal GetDecimal(IXLRow row, Dictionary<string, int> map, string colName)
        {
            return map.ContainsKey(colName)
                ? row.Cell(map[colName]).GetValue<decimal>()
                : 0;
        }






























        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}