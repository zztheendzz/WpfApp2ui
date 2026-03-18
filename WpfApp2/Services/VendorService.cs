using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDTO;

namespace WpfApp2.Services
{
    internal class VendorService
    {
        public DatabaseService _db = new DatabaseService();
    public IEnumerable<VendorDto> GetVendorDTO()
    {
        using var conn = _db.GetConnection();

        string sql = @"
SELECT 
    m.Id,
    m.VendorName,
    CASE 
        WHEN m.IsActive = 1 THEN 1 
        ELSE 0 
    END AS IsActive
FROM Vendor m
    ";
        return conn.Query<VendorDto>(sql);
    }


    public void Delete(int id)
    {
        using var conn = _db.GetConnection();

        string sql = "DELETE FROM Vendor WHERE Id = @Id";

        conn.Execute(sql, new { Id = id });
    }

    public void Edit(VendorDto vendor)
    {
        using var conn = _db.GetConnection();

        string sql = @"
        UPDATE Vendor
        SET 
            VendorName = @VendorName,
            IsActive = @IsActive
        WHERE Id = @Id
        ";

        conn.Execute(sql, vendor);
    }

    public int Add(VendorDto vendor)
    {
        using var conn = _db.GetConnection();

        string sql = @"
    INSERT INTO Vendor (VendorName,IsActive)
    VALUES (@VendorName, @IsActive);

    SELECT last_insert_rowid();
    ";
        return conn.ExecuteScalar<int>(sql, vendor);
    }

} 
}