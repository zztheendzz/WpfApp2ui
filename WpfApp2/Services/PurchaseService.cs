using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDTO;

namespace WpfApp2.Services
{
    public class PurchaseService
    {
        DatabaseService _db = new DatabaseService();

        // SEARCH
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
    m.ModelCode,
    v.VendorName,
    e.EquipmentName,
    c.CategoryName,
    p.Quantity,
    p.UnitPrice,
    p.Quantity * p.UnitPrice AS TotalPrice,
    p.CurrencyCode,
    p.PurchaseDate,
    p.Note
FROM PurchaseHistory p
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


        // GET ALL
        public IEnumerable<PurchaseDto> GetPurchaseDTO()
        {
            using var conn = _db.GetConnection();

            string sql = @"
SELECT
    p.Id,
    m.ModelCode,
    v.VendorName,
    e.EquipmentName,
    p.Quantity,
    p.UnitPrice,
    p.Quantity * p.UnitPrice AS TotalPrice,
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
ORDER BY p.PurchaseDate DESC
";

            return conn.Query<PurchaseDto>(sql);
        }


        // DELETE
        public void Delete(int id)
        {
            using var conn = _db.GetConnection();

            string sql = "DELETE FROM PurchaseHistory WHERE Id = @Id";

            conn.Execute(sql, new { Id = id });
        }


        // EDIT
        public void Edit(PurchaseDto purchase)
        {
            using var conn = _db.GetConnection();

            string sql = @"
UPDATE PurchaseHistory
SET
    ModelId = @ModelId,
    VendorId = @VendorId,
    EquipmentId = @EquipmentId,
    Quantity = @Quantity,
    UnitPrice = @UnitPrice,
    CurrencyCode = @CurrencyCode,
    PurchaseDate = @PurchaseDate,
    Note = @Note
WHERE Id = @Id
";

            conn.Execute(sql, purchase);
        }


        // ADD
        public int Add(PurchaseDto purchase)
        {
            using var conn = _db.GetConnection();

            string sql = @"
INSERT INTO PurchaseHistory
(ModelId, VendorId, EquipmentId, Quantity, UnitPrice, CurrencyCode, PurchaseDate, Note)
VALUES
(@ModelId, @VendorId, @EquipmentId, @Quantity, @UnitPrice, @CurrencyCode, @PurchaseDate, @Note);

SELECT last_insert_rowid();
";

            return conn.ExecuteScalar<int>(sql, purchase);
        }
    }
}