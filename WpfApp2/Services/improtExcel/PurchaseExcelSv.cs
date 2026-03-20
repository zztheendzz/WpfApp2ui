using ClosedXML.Excel;
using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model.modelImportExcel;

namespace WpfApp2.Services.improtExcel
{
    public class PurchaseExcelSv : INotifyPropertyChanged
    {
        // service dùng để kết nối DB
        private DatabaseService _db = new DatabaseService();

        // câu query lấy Vendor (chưa dùng trực tiếp, có thể dư)
        private string vendorDict = "SELECT Id, VendorName AS Name FROM Vendor";

        public void inSertData(string filePath)
        {

            // đọc dữ liệu từ file Excel → list object
            var importPurchases = ReadExcel(filePath);

            // load dictionary 1 lần để tránh query DB nhiều lần trong loop
            var vendorDict = GetDictionary("SELECT Id, VendorName AS Name FROM Vendor");
            var brandDict = GetDictionary("SELECT Id, BrandName AS Name FROM Brand");
            var modelDict = GetDictionary("SELECT Id, ModelName AS Name FROM Model");

            using var conn = _db.GetConnection();

            // list chứa lỗi trong quá trình import
            var errors = new List<string>();

            foreach (var row in importPurchases)
            {
                // ====== CLEAN DATA ======
                // trim khoảng trắng đầu/cuối
                string Normalize(string s) =>
                    s?.Trim().Replace(" ", "").ToUpper();

                var modelName = Normalize(row.ModelName);
                var vendorName = Normalize(row.Vendor);
                var brandName = Normalize(row.Brand);

                // ====== VALIDATE ======

                // check model rỗng
                if (string.IsNullOrEmpty(modelName))
                {
                    errors.Add($"Model empty");
                    continue; // bỏ qua dòng này
                }

                // check vendor tồn tại trong DB (dictionary)
                if (!vendorDict.TryGetValue(vendorName, out int vendorId))
                {
                    errors.Add($"Vendor không tồn tại: {vendorName}");
                    continue;
                }

                // check brand tồn tại
                if (!brandDict.TryGetValue(brandName, out int brandId))
                {
                    errors.Add($"Brand không tồn tại: {brandName}");
                    continue;
                }

                // check model tồn tại
                if (!modelDict.TryGetValue(modelName, out int modelId))
                {
                    // 👉 option 1: báo lỗi nếu không có model
                    errors.Add($"Model không tồn tại: {modelName}");
                    continue;

                    // 👉 option 2: auto insert (đoạn này sẽ không chạy vì đã continue phía trên)

                    modelId = conn.ExecuteScalar<int>(
                        @"INSERT INTO Model(ModelName, BrandId)
                          VALUES(@Name, @BrandId);
                          SELECT CAST(SCOPE_IDENTITY() as int);",
                        new { Name = modelName, VendorId = vendorId, BrandId = brandId }
                    );

                    // thêm model mới vào dictionary để dùng cho các dòng sau
                    modelDict[modelName] = modelId;

                }

                // ====== INSERT PURCHASE ======
                conn.Execute(@"
    INSERT INTO PurchaseHistory
    (ModelId, VendorId, Quantity, UnitPrice, TotalPrice, PurchaseDate)
    VALUES
    (@ModelId, @VendorId, @Quantity, @UnitPrice, @TotalPrice, @PurchaseDate)
", new
                {
                    ModelId = modelId,
                    VendorId = vendorId,
                    Quantity = row.Quantity,
                    UnitPrice = row.UnitPrice,
                    TotalPrice = row.Quantity * row.UnitPrice,
                    PurchaseDate = DateTime.Now
                });
            }

        }


        // tạo dictionary Name -> Id từ query SQL
        public Dictionary<string, int> GetDictionary(string sql)
        {
            using var conn = _db.GetConnection();

            return conn.Query<(int Id, string Name)>(sql)
                .ToDictionary(
                    x => x.Name.Trim(), // chuẩn hoá key (chỉ trim)
                    x => x.Id,
                    StringComparer.OrdinalIgnoreCase // không phân biệt hoa thường
                );
        }


        // đọc file Excel và map sang object ImportPurchase
        public List<ImportPurchase> ReadExcel(string filePath)
        {
            var result = new List<ImportPurchase>();

            using var workbook = new XLWorkbook(filePath);
            var ws = workbook.Worksheet(1);

            // ===== 1. TÌM HEADER =====
            int headerRow = 0;

            foreach (var row in ws.RowsUsed())
            {
                if (row.Cells().Any(c =>
                {
                    var val = c.GetValue<string>().Trim().ToUpper();
                    MessageBox.Show("no la " + val);
                    return val == "NO";
                }))
                {
                    headerRow = row.RowNumber();
                    break;
                }
            }

            if (headerRow == 0)
                throw new Exception("Không tìm thấy header");
            MessageBox.Show("no la " + headerRow);
            // ===== 2. MAP HEADER =====
            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var header = ws.Row(headerRow);

            foreach (var cell in header.Cells())
            {
                var value = cell.GetValue<string>().Trim();

                if (!string.IsNullOrEmpty(value))
                {
                    headerMap[value] = cell.Address.ColumnNumber;
                }
            }

            // ===== 3. READ DATA =====
            int currentRow = headerRow + 1;

            while (true)
            {
                var row = ws.Row(currentRow);

                // 👉 DỪNG CHUẨN (QUAN TRỌNG)
                var modelNameCheck = GetString(row, headerMap, "Model Name");
                if (string.IsNullOrWhiteSpace(modelNameCheck))
                    break;

                var item = new ImportPurchase
                {
                    No = GetInt(row, headerMap, "No"),
                    ModelName = modelNameCheck,
                    Brand = GetString(row, headerMap, "Brand"),
                    ModelCode = GetString(row, headerMap, "Model Code"),
                    Quantity = GetInt(row, headerMap, "Quantity"),
                    UnitPrice = GetDecimal(row, headerMap, "Unit Price"),
                    Vendor = GetString(row, headerMap, "Vendor"),
                    Note = GetString(row, headerMap, "Note")
                };

                result.Add(item);
                currentRow++;
            }

            return result;
        }


        // ====== HELPER ======

        // lấy string theo tên cột
        private string GetString(IXLRow row, Dictionary<string, int> map, string colName)
        {
            return map.ContainsKey(colName)
                ? row.Cell(map[colName]).GetString()
                : "";
        }

        // lấy int theo tên cột
        private int GetInt(IXLRow row, Dictionary<string, int> map, string colName)
        {
            return map.ContainsKey(colName)
                ? row.Cell(map[colName]).GetValue<int>()
                : 0;
        }

        // lấy decimal theo tên cột
        private decimal GetDecimal(IXLRow row, Dictionary<string, int> map, string colName)
        {
            return map.ContainsKey(colName)
                ? row.Cell(map[colName]).GetValue<decimal>()
                : 0;
        }


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        // notify UI khi property thay đổi (nếu dùng binding)
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}