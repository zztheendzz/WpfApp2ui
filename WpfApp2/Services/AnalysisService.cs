using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDTO;

namespace WpfApp2.Services
{
    public class AnalysisService
    {
            private DatabaseService _db = new DatabaseService();

            private string BaseQuery = @"
        SELECT 
            p.Id,
            p.Price,
            p.Date,

            m.Id as ModelId,
            m.Name as ModelName,

            v.Id as VendorId,
            v.Name as VendorName,

            b.Id as BrandId,
            b.Name as BrandName,

            e.Id as EquipmentId,
            e.Name as EquipmentName,

            c.Id as CategoryId,
            c.Name as CategoryName

        FROM PurchaseHistory p
        LEFT JOIN Model m ON p.ModelId = m.Id
        LEFT JOIN Vendor v ON p.VendorId = v.Id
        LEFT JOIN Brand b ON p.BrandId = b.Id
        LEFT JOIN Equipment e ON p.EquipmentId = e.Id
        LEFT JOIN Category c ON p.CategoryId = c.Id
        ";

            public AnalysisService() { }

        public void GetByModel(string modelName)
        {
            //var sql = BaseQuery + " WHERE m.Name LIKE @modelName";

            //return _db.Connection.Query<PurchaseDto>(sql,
            //    new { modelName = "%" + modelName + "%" });
        }
    }
}
