using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.model;
using WpfApp2.modelDTO;
namespace WpfApp2.Services
{
    public class PurchaseService 
    {
        DatabaseService _db = new DatabaseService();

        public IEnumerable<PurchaseDto> Search(
            int? modelId,
            int? vendorId,
            int? equipmentId,
            int? categoryId,
            DateTime? from,
            DateTime? to,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var sql = new StringBuilder(@"
        SELECT 
            p.Id,
            m.ModelCode AS ModelName,
            v.VendorName,
            e.EquipmentName,
            c.CategoryName,
            p.Quantity,
            p.UnitPrice,
            p.Quantity * p.UnitPrice AS TotalPrice,
            p.CurrencyCode,
            p.PurchaseDate,
            p.Note
        FROM Purchase p
        LEFT JOIN Model m ON p.ModelId = m.Id
        LEFT JOIN Vendor v ON p.VendorId = v.Id
        LEFT JOIN Equipment e ON p.EquipmentId = e.Id
        LEFT JOIN Category c ON p.CategoryId = c.Id
        WHERE 1=1
        ");

            if (modelId.HasValue)
                sql.Append(" AND p.ModelId = @modelId");

            if (vendorId.HasValue)
                sql.Append(" AND p.VendorId = @vendorId");

            if (equipmentId.HasValue)
                sql.Append(" AND p.EquipmentId = @equipmentId");

            if (categoryId.HasValue)
                sql.Append(" AND p.CategoryId = @categoryId");

            if (from.HasValue)
                sql.Append(" AND p.PurchaseDate >= @from");

            if (to.HasValue)
                sql.Append(" AND p.PurchaseDate <= @to");

            if (minPrice.HasValue)
                sql.Append(" AND p.UnitPrice >= @minPrice");

            if (maxPrice.HasValue)
                sql.Append(" AND p.UnitPrice <= @maxPrice");

            sql.Append(" ORDER BY p.PurchaseDate DESC");

            using var conn = _db.GetConnection();

            return conn.Query<PurchaseDto>(
                sql.ToString(),
                new { modelId, vendorId, equipmentId, categoryId, from, to, minPrice, maxPrice });
        }
    

            public IEnumerable<PurchaseDto> GetPurchaseDTO()
        {
            using var conn = _db.GetConnection();

            string sql = @"
    SELECT
        p.Id,
        m.ModelName,
        v.VendorName,
        e.EquipmentName,
        p.Quantity,
        p.UnitPrice,
        p.TotalPrice,
        p.CurrencyCode,
        p.PurchaseDate,
        p.CreateAt,
        u.UserName,
        p.Note
    FROM PurchaseHistory p
    LEFT JOIN Model m ON p.ModelId = m.Id
    LEFT JOIN Vendor v ON p.VendorId = v.Id
    LEFT JOIN Equipment e ON p.EquipmentId = e.Id
    LEFT JOIN User u ON p.UserId = u.Id
    ";
            return conn.Query<PurchaseDto>(sql);
        }


        public void Delete(int id)
        {
            using var conn = _db.GetConnection();

            string sql = "DELETE FROM Model WHERE Id = @Id";

            conn.Execute(sql, new { Id = id });
        }

        public void Edit(PurchaseDto purchase)
        {
            using var conn = _db.GetConnection();

            string sql = @"
        UPDATE Model
        SET 
            ModelCode = @ModelCode,
            BrandId = @BrandId,
            IsActive = @IsActive
        WHERE Id = @Id
        ";

            conn.Execute(sql, purchase);
        }

        public int Add(PurchaseDto purchase)
        {
            using var conn = _db.GetConnection();

            string sql = @"
    INSERT INTO Model (ModelCode, BrandId, IsActive)
    VALUES (@ModelCode, @BrandId, @IsActive);

    SELECT last_insert_rowid();
    ";
            return conn.ExecuteScalar<int>(sql, purchase);
        }

    }


}
