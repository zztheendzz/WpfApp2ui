using ClosedXML.Excel;
using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using WpfApp2.model.modelImportExcel;
using System;

namespace WpfApp2.Services.improtExcel
{
    public class PurchaseExcelSv : INotifyPropertyChanged
    {
        private DatabaseService _db = new DatabaseService();

        // ===== NORMALIZE =====
        private string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";

            return new string(s
                .Trim()
                .ToUpper()
                .Where(c => !char.IsWhiteSpace(c)) // 🔥 ăn hết mọi loại space
                .ToArray());
        }

        // ===== MAIN INSERT =====
        public void inSertData(string filePath)
        {
            try
            {
                var importPurchases = ReadExcel(filePath);

                if (importPurchases.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu!");
                    return;
                }

                var vendorDict = GetDictionary("SELECT Id, VendorName AS Name FROM Vendor");
                var brandDict = GetDictionary("SELECT Id, BrandName AS Name FROM Brand");

                // 🔥 FIX: dùng ModelCode làm key
                var modelDict = GetDictionary("SELECT Id, ModelCode AS Name FROM Model");

                using var conn = _db.GetConnection();

                int success = 0;
                string lastVendor = null;

                foreach (var row in importPurchases)
                {
                    string createAt = GetCurrentDateTime();
                    var modelName = Normalize(row.ModelName);
                    var modelCode = Normalize(row.ModelCode);
                    var vendorName = Normalize(row.Vendor);

                    // ===== HANDLE VENDOR TRỐNG =====
                    if (string.IsNullOrEmpty(vendorName))
                        vendorName = lastVendor;
                    else
                        lastVendor = vendorName;

                    if (string.IsNullOrEmpty(modelName))
                        continue;

                    int vendorId = GetOrCreateVendor(conn, vendorDict, row.Vendor);
                    int brandId = GetOrCreateBrand(conn, brandDict, row.Brand);

                    // 🔥 FIX: truyền modelCode
                    int modelId = GetOrCreateModel(
                        conn,
                        modelDict,
                        row.ModelName,
                        row.ModelCode,
                        brandId
                    );

                    conn.Execute(@"
INSERT INTO PurchaseHistory
(ModelId, VendorId, Quantity, UnitPrice, TotalPrice, PurchaseDate,CreateAt)
VALUES
(@ModelId, @VendorId, @Quantity, @UnitPrice, @TotalPrice, @PurchaseDate,@CreateAt)",
                    new
                    {
                        ModelId = modelId,
                        VendorId = vendorId,
                        Quantity = row.Quantity,
                        UnitPrice = row.UnitPrice,
                        TotalPrice = row.Quantity * row.UnitPrice,
                        PurchaseDate = DateTime.Now,
                        CreateAt= createAt
                    });

                    success++;
                }

                MessageBox.Show($"Insert thành công: {success}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }

        // ===== GET OR CREATE =====

        private int GetOrCreateVendor(IDbConnection conn, Dictionary<string, int> dict, string name)
        {
            var key = Normalize(name);
            if (string.IsNullOrEmpty(key)) return 0;

            if (dict.TryGetValue(key, out int id))
                return id;

            id = conn.ExecuteScalar<int>(@"
INSERT INTO Vendor(VendorName, IsActive)
VALUES(@Name, 1);
SELECT last_insert_rowid();",
                new { Name = name });

            dict[key] = id;
            return id;
        }

        private int GetOrCreateBrand(IDbConnection conn, Dictionary<string, int> dict, string name)
        {
            var key = Normalize(name);
            if (string.IsNullOrEmpty(key)) return 0;

            if (dict.TryGetValue(key, out int id))
                return id;

            id = conn.ExecuteScalar<int>(@"
INSERT INTO Brand(BrandName, IsActive)
VALUES(@Name, 1);
SELECT last_insert_rowid();",
                new { Name = name });

            dict[key] = id;
            return id;
        }

        private int GetOrCreateModel(
            IDbConnection conn,
            Dictionary<string, int> dict,
            string name,
            string modelCode,
            int brandId)
        {
            // 🔥 FIX: dùng ModelCode làm key
            var key = Normalize(modelCode);

            if (string.IsNullOrEmpty(key)) return 0;

            if (dict.TryGetValue(key, out int id))
                return id;

            id = conn.ExecuteScalar<int>(@"
INSERT INTO Model(ModelName, ModelCode, BrandId, IsActive)
VALUES(@Name, @ModelCode, @BrandId,1);
SELECT last_insert_rowid();",
                new
                {
                    Name = name,
                    ModelCode = modelCode,
                    BrandId = brandId
                });

            dict[key] = id;
            return id;
        }

        // ===== DICTIONARY =====
        public Dictionary<string, int> GetDictionary(string sql)
        {
            using var conn = _db.GetConnection();

            return conn.Query<(int Id, string Name)>(sql)
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .ToDictionary(
                    x => Normalize(x.Name),
                    x => x.Id
                );
        }

        // ===== READ EXCEL ===== (giữ nguyên)
        public List<ImportPurchase> ReadExcel(string filePath)
        {
            var result = new List<ImportPurchase>();

            using var workbook = new XLWorkbook(filePath);
            var ws = workbook.Worksheet(1);

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
                throw new Exception("Không tìm thấy header");

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

        // ===== HELPER =====
        private string GetString(IXLRow row, Dictionary<string, int> map, string colName)
        {
            var key = Normalize(colName);
            return map.ContainsKey(key) ? row.Cell(map[key]).GetString() : "";
        }

        private int GetInt(IXLRow row, Dictionary<string, int> map, string colName)
        {
            var key = Normalize(colName);
            return map.ContainsKey(key) ? row.Cell(map[key]).GetValue<int>() : 0;
        }

        private decimal GetDecimal(IXLRow row, Dictionary<string, int> map, string colName)
        {
            var key = Normalize(colName);
            return map.ContainsKey(key) ? row.Cell(map[key]).GetValue<decimal>() : 0;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
        public string GetCurrentDateTime()
        {
            return DateTime.Now.ToString("HH:mm dd/MM/yyyy");
        }
    }
}