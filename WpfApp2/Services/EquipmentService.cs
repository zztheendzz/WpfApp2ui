using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.modelDTO;

namespace WpfApp2.Services
{
    internal class EquipmentService
    {
        public DatabaseService _db = new DatabaseService();
        public IEnumerable<EquipmentDto> GetEquipmentDto()
        {
            using var conn = _db.GetConnection();

            string sql = @"
                SELECT 
                    m.Id,
                    m.EquipmentName,
                    m.IsActive
                FROM Equipment m
                where m.IsActive = 1
                    ";
            return conn.Query<EquipmentDto>(sql);
        }


        public void Delete(int id)
        {
            using var conn = _db.GetConnection();

            string sql = @"
        UPDATE Equipment
        SET 
            IsActive = 0
        WHERE Id = @Id
        ";

            conn.Execute(sql, new { Id = id });
        }

        public void Edit(EquipmentDto equipment)
        {
            using var conn = _db.GetConnection();

            string sql = @"
        UPDATE Equipment
        SET 
            EquipmentName = @EquipmentName
        WHERE Id = @Id
        ";

            conn.Execute(sql, equipment);
        }

        public int Add(EquipmentDto equipment)
        {
            using var conn = _db.GetConnection();

            string sql = @"
    INSERT INTO Equipment (EquipmentName, IsActive)
    VALUES (@EquipmentName, @IsActive);

    SELECT last_insert_rowid();
    ";
            return conn.ExecuteScalar<int>(sql, equipment);
        }

    }
}