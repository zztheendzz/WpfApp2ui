using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using WpfApp2.model;
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
            //int? categoryId,
            DateTime? from,
            DateTime? to,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var sql = new StringBuilder(@"
                SELECT 
                    p.Id,
                    m.ModelName AS ModelName,
                    v.VendorName,
                    e.EquipmentName,
                    c.CategoryName,
                    p.Quantity,
                    p.UnitPrice,
                    p.Quantity * p.UnitPrice AS TotalPrice,
                    p.CurrencyId,
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

            //if (categoryId.HasValue)
            //    sql.Append(" AND p.CategoryId = @categoryId");

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
                new { modelId, vendorId, equipmentId, /*categoryId,*/ from, to, minPrice, maxPrice });
        }



        public IEnumerable<PurchaseDto> GetPurchaseDTO()
        {
            using var conn = _db.GetConnection();

            string sql = @"
                    SELECT
                        p.Id,
                        p.Quantity,
                        p.UnitPrice,
                        p.Quantity * p.UnitPrice AS TotalPrice,
                        p.PurchaseDate,
                        p.Note,
                        p.CreateAt,

                        


                        p.ModelId,
                        m.ModelName,
                        m.ModelCode,
                        m.Image,

                        p.VendorId,
                        v.VendorName,

                        p.EquipmentId,
                        e.EquipmentName,

                        p.CurrencyId,
                        c.CurrencyName

                    FROM PurchaseHistory p
                    LEFT JOIN Model m ON p.ModelId = m.Id
                    LEFT JOIN Vendor v ON p.VendorId = v.Id
                    LEFT JOIN Equipment e ON p.EquipmentId = e.Id
                    LEFT JOIN Currency c ON p.CurrencyId = c.Id
                    ORDER BY p.PurchaseDate DESC
";

            /*
                             SELECT 
                                m.Id,
                                m.ModelCode,
                                m.BrandId,        -- FK (int)
                                m.ModelName,
                                b.BrandName       -- lấy từ bảng Brand
                            FROM Model m
                            LEFT JOIN Brand b ON m.BrandId = b.Id
                            WHERE m.IsActive = 1



             */

            return conn.Query<PurchaseDto>(sql);
        }

        public void deleteAll()
        {
            using var conn = _db.GetConnection();

            string sql = "DELETE FROM PurchaseHistory";

            conn.Execute(sql);
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
            conn.Open();

            using var tran = conn.BeginTransaction();

            try
            {
                // 🔥 1. Ensure FK
                purchase.ModelId = EnsureModel(purchase.ModelCode, purchase.ModelName,purchase.BrandName, conn, tran);
                purchase.VendorId = EnsureVendor(purchase.VendorName, conn, tran);
                purchase.EquipmentId = EnsureEquipment(purchase.EquipmentName, conn, tran);

                // 🔥 2. Update
                string sql = @"
            UPDATE PurchaseHistory
            SET
                ModelId = @ModelId,
                VendorId = @VendorId,
                EquipmentId = @EquipmentId,
                Quantity = @Quantity,
                UnitPrice = @UnitPrice,
                PurchaseDate = @PurchaseDate,
                Note = @Note
            WHERE Id = @Id";

                conn.Execute(sql, purchase, tran);

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }




        // ADD
        public int Add(PurchaseDto purchase)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            try
            {
                // 🔹 1. Vendor
                if (!string.IsNullOrWhiteSpace(purchase.VendorName))
                {
                    purchase.VendorId = EnsureVendor(
                        purchase.VendorName,
                        conn,
                        tran
                    );
                }

                // 🔹 2. Brand
                //int brandId = 0;
                //if (!string.IsNullOrWhiteSpace(purchase.BrandName))
                //{
                //    brandId = EnsureBrand(
                //        purchase.BrandName,
                //        conn,
                //        tran
                //    );
                //}

                // 🔹 3. Model (bắt buộc phải có BrandId)
                if (!string.IsNullOrWhiteSpace(purchase.ModelName))
                {
                    purchase.ModelId = EnsureModel(
                        purchase.ModelName,
                        purchase.ModelName,
                        purchase.BrandName,
                        conn,
                        tran
                    );
                }

                // 🔥 4. Insert Purchase (KHÔNG dùng BrandId)
                string sql = @"
            INSERT INTO PurchaseHistory
            (ModelId, VendorId, EquipmentId, Quantity, UnitPrice, TotalPrice, CurrencyId, PurchaseDate, CreateAt, UserId, Note)
            VALUES
            (@ModelId, @VendorId, @EquipmentId, @Quantity, @UnitPrice, @TotalPrice, 1, @PurchaseDate, @CreateAt, @UserId, @Note);

            SELECT last_insert_rowid();
        ";

                var newId = conn.ExecuteScalar<int>(sql, new
                {
                    purchase.ModelId,
                    purchase.VendorId,
                    purchase.EquipmentId,
                    purchase.Quantity,
                    purchase.UnitPrice,

                    // 🔥 tính luôn tránh lệch DB
                    TotalPrice = purchase.Quantity * purchase.UnitPrice,

                    purchase.CurrencyId,
                    purchase.PurchaseDate,
                    purchase.UserId,
                    purchase.Note,

                    CreateAt = DateTime.Now
                }, tran);

                tran.Commit();
                return newId;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
        private int EnsureVendor(string name, IDbConnection conn, IDbTransaction tran)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Vendor is required");

            var id = conn.QueryFirstOrDefault<int?>(
                "SELECT Id FROM Vendor WHERE VendorName = @name",
                new { name }, tran);

            if (id.HasValue)
                return id.Value;

            return conn.ExecuteScalar<int>(
                @"INSERT INTO Vendor (VendorName, IsActive)
          VALUES (@name, 1);
          SELECT last_insert_rowid();",
                new { name }, tran);
        }
        private int EnsureEquipment(string name, IDbConnection conn, IDbTransaction tran)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Equipment is required");

            var id = conn.QueryFirstOrDefault<int?>(
                "SELECT Id FROM Equipment WHERE EquipmentName = @name",
                new { name }, tran);

            if (id.HasValue)
                return id.Value;

            return conn.ExecuteScalar<int>(
                @"INSERT INTO Equipment (EquipmentName, IsActive)
          VALUES (@name, 1);
          SELECT last_insert_rowid();",
                new { name }, tran);
        }
        private int EnsureBrand(string name, IDbConnection conn, IDbTransaction tran)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Brand is required");

            name = name.Trim();

            var id = conn.QueryFirstOrDefault<int?>(
                "SELECT Id FROM Brand WHERE BrandName = @name",
                new { name }, tran);

            if (id.HasValue)
                return id.Value;

            return conn.ExecuteScalar<int>(
                @"INSERT INTO Brand (BrandName, IsActive)
          VALUES (@name, 1);
          SELECT last_insert_rowid();",
                new { name }, tran);
        }
        private int EnsureModel(string code, string name, string brandName,
                                IDbConnection conn, IDbTransaction tran)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new Exception("ModelCode is required");

            var id = conn.QueryFirstOrDefault<int?>(
                "SELECT Id FROM Model WHERE ModelCode = @code",
                new { code }, tran);

            if (id.HasValue)
                return id.Value;

            return conn.ExecuteScalar<int>(
                @"INSERT INTO Model (ModelCode, ModelName, IsActive)
          VALUES (@code, @name, 1);
          SELECT last_insert_rowid();",
                new { code, name }, tran);
        }

    }
}