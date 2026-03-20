using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.Services;
using Dapper;
namespace WpfApp2.viewmodel.dialogVm
{
    internal class EditVm
    {

        private readonly DatabaseService _db = new DatabaseService();

        public object Model { get; set; }

        // FK config
        public Dictionary<string, (string Table, string Display)> LookupMap = new()
    {
        {"ModelId", ("Model", "ModelName")},
        {"BrandId", ("Brand", "BrandName")},
        {"CategoryId", ("Category", "CategoryName")},
        {"VendorId", ("Vendor", "VendorName")},
        {"EquipmentId", ("Equipment", "EquipmentName")},
        {"UserId", ("User", "UserName")},
        {"CurrencyCode", ("Currency", "Name")}
    };

        private Dictionary<string, List<dynamic>> _cache = new();

        public EditVm(object model)
        {
            Model = model;
        }

        public List<dynamic> LoadLookup(string table, string display)
        {

            string key = $"{table}_{display}";

            if (_cache.ContainsKey(key))
                return _cache[key];

            using var conn = _db.GetConnection();

            var data = conn.Query($"SELECT * FROM {table}").ToList();

            _cache[key] = data;

            return data;
        }

    }
}
