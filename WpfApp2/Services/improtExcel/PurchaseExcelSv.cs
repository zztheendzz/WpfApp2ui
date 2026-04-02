using ClosedXML.Excel;
using Dapper;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using WpfApp2.model.modelImportExcel;
using WpfApp2.Services.sessionService;
using System.Collections.Generic;
using System.Linq;
using System;

namespace WpfApp2.Services.improtExcel
{
    public class PurchaseExcelSv : INotifyPropertyChanged
    {
        private DatabaseService _db = new DatabaseService();
        int UserId = SessionService.CurrentUser.Id;

        // ===== 1. MỚI: LẤY DANH SÁCH SHEET =====
        public List<string> GetSheetNames(string filePath)
        {
            using var workbook = new XLWorkbook(filePath);
            return workbook.Worksheets.Select(x => x.Name).ToList();
        }

        // ===== NORMALIZE =====
        private string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            return new string(s.Trim().ToUpper().Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        // ===== MAIN INSERT (Cập nhật tham số) =====
        public void inSertData(string filePath, string sheetName)
        {
            try
            {
                // Truyền sheetName vào đây
                var importPurchases = ReadExcel(filePath, sheetName);

                if (importPurchases.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu trong sheet đã chọn!");
                    return;
                }

                var vendorDict = GetDictionary("SELECT Id, VendorName AS Name FROM Vendor");
                var brandDict = GetDictionary("SELECT Id, BrandName AS Name FROM Brand");
                var modelDict = GetDictionary("SELECT Id, ModelCode AS Name FROM Model");

                using var conn = _db.GetConnection();
                int success = 0;
                string lastVendor = null;

                foreach (var row in importPurchases)
                {
                    string createAt = GetCurrentDateTime();
                    var modelName = Normalize(row.ModelName);
                    var vendorName = Normalize(row.Vendor);

                    if (string.IsNullOrEmpty(vendorName))
                        vendorName = lastVendor;
                    else
                        lastVendor = vendorName;

                    if (string.IsNullOrEmpty(modelName)) continue;

                    int vendorId = GetOrCreateVendor(conn, vendorDict, row.Vendor);
                    int brandId = GetOrCreateBrand(conn, brandDict, row.Brand);
                    int modelId = GetOrCreateModel(conn, modelDict, row.ModelName, row.ModelCode, brandId);

                    conn.Execute(@"
                        INSERT INTO PurchaseHistory 
                        (ModelId, VendorId, Quantity, UnitPrice, TotalPrice, PurchaseDate, CreateAt, UserId)
                        VALUES 
                        (@ModelId, @VendorId, @Quantity, @UnitPrice, @TotalPrice, @PurchaseDate, @CreateAt, @UserId)",
                    new
                    {
                        ModelId = modelId,
                        VendorId = vendorId,
                        Quantity = row.Quantity,
                        UnitPrice = row.UnitPrice,
                        TotalPrice = row.Quantity * row.UnitPrice,
                        PurchaseDate = GetCurrentDate(),
                        CreateAt = createAt,
                        UserId = UserId
                    });

                    success++;
                }

                MessageBox.Show($"Đã import thành công {success} dòng từ sheet '{sheetName}'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message); // Ném lỗi để ViewModel bắt và hiển thị vào Message
            }
        }

        // ===== READ EXCEL (Cập nhật tham số sheetName) =====
        public List<ImportPurchase> ReadExcel(string filePath, string sheetName)
        {
            var result = new List<ImportPurchase>();

            using var workbook = new XLWorkbook(filePath);

            // Lấy sheet được chọn thay vì mặc định sheet 1
            if (!workbook.Worksheets.Contains(sheetName))
                throw new Exception($"Không tìm thấy sheet có tên '{sheetName}'");

            var ws = workbook.Worksheet(sheetName);

            int headerRow = 0;
            foreach (var row in ws.RowsUsed())
            {
                if (row.Cells().Any(c => Normalize(c.GetValue<string>()) == "NO"))
                {
                    headerRow = row.RowNumber();
                    break;
                }
            }

            if (headerRow == 0)
                throw new Exception("Không tìm thấy tiêu đề (cột 'NO') trong sheet này.");

            var headerMap = new Dictionary<string, int>();
            var header = ws.Row(headerRow);

            foreach (var cell in header.Cells())
            {
                var key = Normalize(cell.GetValue<string>());
                if (!string.IsNullOrEmpty(key))
                    headerMap[key] = cell.Address.ColumnNumber;
            }

            int currentRow = headerRow + 1;
            int lastRow = ws.LastRowUsed().RowNumber();

            while (currentRow <= lastRow)
            {
                var row = ws.Row(currentRow);
                var modelName = GetString(row, headerMap, "MODELNAME");

                if (string.IsNullOrWhiteSpace(modelName))
                {
                    currentRow++;
                    continue;
                }

                result.Add(new ImportPurchase
                {
                    No = GetInt(row, headerMap, "NO"),
                    ModelName = modelName,
                    Brand = GetString(row, headerMap, "BRAND"),
                    ModelCode = GetString(row, headerMap, "MODELCODE"),
                    Quantity = GetInt(row, headerMap, "QUANTITY"),
                    UnitPrice = GetDecimal(row, headerMap, "UNITPRICE"),
                    Vendor = GetString(row, headerMap, "VENDOR"),
                    Note = GetString(row, headerMap, "NOTE")
                });

                currentRow++;
            }

            return result;
        }

        // --- Các hàm GetOrCreate và Helper giữ nguyên như cũ ---
        private int GetOrCreateVendor(IDbConnection conn, Dictionary<string, int> dict, string name)
        {
            var key = Normalize(name);
            if (string.IsNullOrEmpty(key)) return 0;
            if (dict.TryGetValue(key, out int id)) return id;
            id = conn.ExecuteScalar<int>(@"INSERT INTO Vendor(VendorName, IsActive) VALUES(@Name, 1); SELECT last_insert_rowid();", new { Name = name });
            dict[key] = id;
            return id;
        }

        private int GetOrCreateBrand(IDbConnection conn, Dictionary<string, int> dict, string name)
        {
            var key = Normalize(name);
            if (string.IsNullOrEmpty(key)) return 0;
            if (dict.TryGetValue(key, out int id)) return id;
            id = conn.ExecuteScalar<int>(@"INSERT INTO Brand(BrandName, IsActive) VALUES(@Name, 1); SELECT last_insert_rowid();", new { Name = name });
            dict[key] = id;
            return id;
        }

        private int GetOrCreateModel(IDbConnection conn, Dictionary<string, int> dict, string name, string modelCode, int brandId)
        {
            var key = Normalize(modelCode);
            if (string.IsNullOrEmpty(key)) return 0;
            if (dict.TryGetValue(key, out int id)) return id;
            id = conn.ExecuteScalar<int>(@"INSERT INTO Model(ModelName, ModelCode, BrandId, IsActive) VALUES(@Name, @ModelCode, @BrandId, 1); SELECT last_insert_rowid();",
                new { Name = name, ModelCode = modelCode, BrandId = brandId });
            dict[key] = id;
            return id;
        }

        public Dictionary<string, int> GetDictionary(string sql)
        {
            using var conn = _db.GetConnection();
            return conn.Query<(int Id, string Name)>(sql)
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .ToDictionary(x => Normalize(x.Name), x => x.Id);
        }

        private string GetString(IXLRow row, Dictionary<string, int> map, string colName)
        {
            var key = Normalize(colName);
            return map.ContainsKey(key) ? row.Cell(map[key]).GetString() : "";
        }

        private int GetInt(IXLRow row, Dictionary<string, int> map, string colName)
        {
            var key = Normalize(colName);
            return map.ContainsKey(key) ? (row.Cell(map[key]).TryGetValue<int>(out int val) ? val : 0) : 0;
        }

        private decimal GetDecimal(IXLRow row, Dictionary<string, int> map, string colName)
        {
            var key = Normalize(colName);
            return map.ContainsKey(key) ? (row.Cell(map[key]).TryGetValue<decimal>(out decimal val) ? val : 0) : 0;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        public string GetCurrentDateTime() => DateTime.Now.ToString("HH:mm dd-MM-yyyy");
        public string GetCurrentDate() => DateTime.Now.ToString("yyyy-MM-dd");
    }
}