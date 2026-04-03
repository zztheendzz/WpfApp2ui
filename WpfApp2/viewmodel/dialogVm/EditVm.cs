using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows; // Thêm để dùng MessageBox
using WpfApp2.Services;
using WpfApp2.Services.exception;

namespace WpfApp2.viewmodel.dialogVm
{
    internal class EditVm
    {
        private readonly DatabaseService _db = new DatabaseService();

        public object Model { get; set; }

        // Cấu hình Foreign Key (FK)
        public Dictionary<string, (string Table, string Display)> LookupMap = new()
        {
            {"ModelId", ("Model", "ModelName")},
            {"BrandId", ("Brand", "BrandName")},
            {"CategoryId", ("Category", "CategoryName")},
            {"VendorId", ("Vendor", "VendorName")},
            {"EquipmentId", ("Equipment", "EquipmentName")},
            {"UserId", ("User", "UserName")},
            {"CurrencyName", ("Currency", "Name")}
        };

        private Dictionary<string, List<dynamic>> _cache = new();

        public EditVm(object model)
        {
            Model = model;
        }

        public List<dynamic> LoadLookup(string table, string display)
        {
            string key = $"{table}_{display}";

            // Kiểm tra cache để tránh truy vấn DB nhiều lần cùng một phiên
            if (_cache.ContainsKey(key))
                return _cache[key];

            try
            {
                using var conn = _db.GetConnection();

                // Lưu ý: Tên bảng và cột được truyền trực tiếp qua nội suy chuỗi 
                // vì đây là cấu hình nội bộ từ LookupMap (không phải input từ user)
                var data = conn.Query($"SELECT * FROM {table}").ToList();

                _cache[key] = data;
                return data;
            }
            catch (DatabaseLockedException)
            {
                // Thông báo lỗi nếu DB bị khóa khi đang load danh sách chọn (ComboBox)
                MessageBox.Show($"Hệ thống đang bận, không thể tải danh sách dữ liệu từ bảng {table}. Vui lòng đóng cửa sổ và thử lại.",
                                "Cơ sở dữ liệu bận");
                return new List<dynamic>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi không xác định khi tải dữ liệu: {ex.Message}");
                return new List<dynamic>();
            }
        }
    }
}