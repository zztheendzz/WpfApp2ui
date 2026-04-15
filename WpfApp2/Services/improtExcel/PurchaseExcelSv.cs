using ClosedXML.Excel;
using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using WpfApp2.model.modelImportExcel;
using WpfApp2.Services.sessionService;

namespace WpfApp2.Services.improtExcel
{
    public class PurchaseExcelSv : INotifyPropertyChanged
    {
        private DatabaseService _db = new DatabaseService();
        int UserId = SessionService.CurrentUser.Id;

        // Lấy danh sách Sheet để hiển thị lên ComboBox
        public List<string> GetSheetNames(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                return workbook.Worksheets.Select(x => x.Name).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Không thể đọc file Excel: " + ex.Message);
            }
        }

        // Chuẩn hóa chuỗi để so sánh tiêu đề (Viết hoa, bỏ dấu cách)
        private string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            return new string(s.Trim().ToUpper().Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        // Hàm kiểm tra ô có phải tiêu đề "Tên hàng" hay không (Hỗ trợ Việt - Hàn - Anh)
        private bool IsModelNameHeader(string text)
        {
            string n = Normalize(text);
            return n.Contains("TENHANG") || n.Contains("제품명") || n.Contains("MODELNAME");
        }

        public string GetProjectName(string filePath, string sheetName)
        {
            using var workbook = new XLWorkbook(filePath);
            var ws = workbook.Worksheet(sheetName);

            // Quét vài dòng đầu để tìm ô có chứa "Project name:"
            for (int i = 1; i <= 5; i++)
            {
                var cellValue = ws.Cell(i, 1).GetValue<string>();
                if (cellValue.Contains("Project name:"))
                {
                    // Lấy phần tên sau dấu ":"
                    return cellValue.Replace("Project name:", "").Trim();
                }

            }
            var dateTime = DateTime.Now.ToString("HH:mm:ss");
            string rs = "Unknown Project" + dateTime;
            return rs;
        }
        private int GetOrCreateEquipment(IDbConnection conn, string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName) || projectName == "Unknown Project")
                return 0;

            // Chuẩn hóa tên để tìm kiếm
            string normalizedName = projectName.Trim();

            // Kiểm tra xem đã có Equipment này chưa
            int? existingId = conn.QueryFirstOrDefault<int?>(
                "SELECT Id FROM Equipment WHERE EquipmentName = @Name",
                new { Name = normalizedName });

            if (existingId.HasValue) return existingId.Value;

            // Nếu chưa có, tiến hành tạo mới
            return conn.ExecuteScalar<int>(@"
        INSERT INTO Equipment (EquipmentName, IsActive) 
        VALUES (@Name, 1); 
        SELECT last_insert_rowid();",
                new { Name = normalizedName });
        }

        // Thực hiện Insert dữ liệu vào Database (non-blocking wrapper)
        public async void inSertData(string filePath, string sheetName)
        {
            try
            {
                int success = await inSertDataAsync(filePath, sheetName, CancellationToken.None).ConfigureAwait(false);
                // Show result on UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Dự án: {GetProjectName(filePath, sheetName)}\nĐã import thành công {success} dòng.");
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
            }
        }

        // Async version with cancellation support. Returns number of imported rows.
        public async Task<int> inSertDataAsync(string filePath, string sheetName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("filePath is required", nameof(filePath));

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 1. Lấy Project Name từ Excel trước
                string projectName = GetProjectName(filePath, sheetName);

                // 2. Đọc dữ liệu hàng hóa (IO-bound) - run on threadpool to avoid blocking caller
                var importPurchases = await Task.Run(() => ReadExcel(filePath, sheetName), cancellationToken).ConfigureAwait(false);

                if (importPurchases == null || importPurchases.Count == 0)
                {
                    throw new InvalidOperationException("Không tìm thấy dữ liệu hợp lệ!");
                }

                var vendorDict = GetDictionary("SELECT Id, VendorName AS Name FROM Vendor");
                var brandDict = GetDictionary("SELECT Id, BrandName AS Name FROM Brand");
                var modelDict = GetDictionary("SELECT Id, ModelCode AS Name FROM Model");

                using var conn = _db.GetConnection();
                if (conn.State == ConnectionState.Closed) conn.Open();
                using var transaction = conn.BeginTransaction();

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // 3. Lấy hoặc Tạo mới EquipmentId từ Project Name
                    int equipmentId = GetOrCreateEquipment(conn, projectName);

                    int success = 0;
                    foreach (var row in importPurchases)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        int vendorId = GetOrCreateVendor(conn, vendorDict, row.Vendor);
                        int brandId = GetOrCreateBrand(conn, brandDict, row.Brand);
                        int modelId = GetOrCreateModel(conn, modelDict, row.ModelName, row.ModelCode, brandId);

                        // 4. Insert vào PurchaseHistory (Thêm cột EquipmentId)
                        conn.Execute(@"
                    INSERT INTO PurchaseHistory 
                    (ModelId, VendorId, EquipmentId, Quantity, UnitPrice, TotalPrice, PurchaseDate, CreateAt, UserId,CurrencyId)
                    VALUES 
                    (@ModelId, @VendorId, @EquipmentId, @Quantity, @UnitPrice, @TotalPrice, @PurchaseDate, @CreateAt, @UserId,@CurrencyId)",
                            new
                            {
                                ModelId = modelId,
                                VendorId = vendorId,
                                EquipmentId = equipmentId, // Gắn ID thiết bị/dự án vào đây
                                Quantity = row.Quantity,
                                UnitPrice = row.UnitPrice,
                                TotalPrice = row.Quantity * row.UnitPrice,
                                PurchaseDate = DateTime.Now.ToString("yyyy-MM-dd"),
                                CreateAt = DateTime.Now.ToString("HH:mm dd-MM-yyyy"),
                                UserId = UserId,
                                CurrencyId = 1 // hardcode tạm thời

                            }, transaction);

                        success++;
                    }

                    transaction.Commit();
                    return success;
                }
                catch (IOException ioEx)
                {
                    transaction.Rollback();
                    throw new IOException("Lỗi IO khi ghi vào database: " + ioEx.Message, ioEx);
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    transaction.Rollback();
                    throw new UnauthorizedAccessException("Quyền truy cập bị từ chối: " + uaEx.Message, uaEx);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Lỗi Database: " + ex.Message, ex);
                }
            }
            catch (IOException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi import dữ liệu: " + ex.Message, ex);
            }
        }

        // Đọc dữ liệu từ Excel với cơ chế quét cột động
        public List<ImportPurchase> ReadExcel(string filePath, string sheetName)
        {
            var result = new List<ImportPurchase>();
            using var workbook = new XLWorkbook(filePath);

            if (!workbook.Worksheets.Contains(sheetName))
                throw new Exception($"Không tìm thấy sheet '{sheetName}'");

            var ws = workbook.Worksheet(sheetName);
            IXLRow headerRow = null;
            var headerMap = new Dictionary<string, int>();

            // Bước 1: Tìm hàng tiêu đề (Quét 50 hàng đầu tiên)
            foreach (var row in ws.RowsUsed().Take(50))
            {
                if (row.CellsUsed().Any(c => IsModelNameHeader(c.GetValue<string>())))
                {
                    headerRow = row;
                    foreach (var cell in row.CellsUsed())
                    {
                        string txt = Normalize(cell.GetValue<string>());
                        int col = cell.Address.ColumnNumber;

                        if (txt.Contains("STT") || txt.Contains("NO")) headerMap["NO"] = col;
                        else if (txt.Contains("TENHANG") || txt.Contains("제품명") || txt.Contains("MODELNAME")) headerMap["MODELNAME"] = col;
                        else if (txt.Contains("NHANHIEU") || txt.Contains("브랜드") || txt.Contains("BRAND")) headerMap["BRAND"] = col;
                        else if (txt.Contains("MAHANG") || txt.Contains("CODE")|| txt.Contains("기능,규격") || txt.Contains("CHUCNANGQUYCACH")) headerMap["MODELCODE"] = col;
                        else if (txt.Contains("DONGIA") || txt.Contains("단가") || txt.Contains("UNITPRICE")) headerMap["UNITPRICE"] = col;
                        else if (txt.Contains("SOLUONG") || txt.Contains("수량") || txt.Contains("QUANTITY")) headerMap["QUANTITY"] = col;
                        else if (txt.Contains("NHACUNG") || txt.Contains("공급업체") || txt.Contains("VENDOR")) headerMap["VENDOR"] = col;
                        else if (txt.Contains("GHICHU") || txt.Contains("비고") || txt.Contains("NOTE")) headerMap["NOTE"] = col;
                    }
                    break;
                }
            }

            if (headerRow == null)
                throw new Exception("Không tìm thấy hàng tiêu đề có chứa 'Tên hàng' hoặc '제품명'.");

            // Bước 2: Đọc dữ liệu từ hàng kế tiếp
            int startRow = headerRow.RowNumber() + 1;
            int lastRow = ws.LastRowUsed().RowNumber();

            for (int r = startRow; r <= lastRow; r++)
            {
                var row = ws.Row(r);
                string modelName = GetCellValue(row, headerMap, "MODELNAME");

                // Bỏ qua nếu hàng trống hoặc lỡ đọc phải hàng tiêu đề phụ (nếu có)
                if (string.IsNullOrWhiteSpace(modelName) || IsModelNameHeader(modelName)) continue;

                result.Add(new ImportPurchase
                {
                    No = ParseInt(GetCellValue(row, headerMap, "NO")),
                    ModelName = modelName.Trim(),
                    Brand = GetCellValue(row, headerMap, "BRAND").Trim(),
                    ModelCode = GetCellValue(row, headerMap, "MODELCODE").Trim(),
                    Quantity = ParseInt(GetCellValue(row, headerMap, "QUANTITY")),
                    UnitPrice = ParseDecimal(GetCellValue(row, headerMap, "UNITPRICE")),
                    Vendor = GetCellValue(row, headerMap, "VENDOR").Trim(),
                    Note = GetCellValue(row, headerMap, "NOTE").Trim()
                });
            }

            return result;
        }

        #region Helpers
        private string GetCellValue(IXLRow row, Dictionary<string, int> map, string key)
        {
            return map.ContainsKey(key) ? row.Cell(map[key]).GetValue<string>() : "";
        }

        private int ParseInt(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            var styles = System.Globalization.NumberStyles.Integer | System.Globalization.NumberStyles.AllowThousands;
            return int.TryParse(s, styles, CultureInfo.CurrentCulture, out int val) ? val : 0;
        }

        private decimal ParseDecimal(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            var styles = System.Globalization.NumberStyles.Number | System.Globalization.NumberStyles.AllowThousands;
            return decimal.TryParse(s, styles, CultureInfo.CurrentCulture, out decimal val) ? val : 0;
        }

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
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}