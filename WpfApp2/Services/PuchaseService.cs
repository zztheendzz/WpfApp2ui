using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.model;

namespace WpfApp2.Services
{
    class PurchaseService
    {
        public List<PurchaseHistory> GetAll()
        {
            var list = new List<PurchaseHistory>();

            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT * FROM PurchaseHistory";

            using var reader = cmd.ExecuteReader();


            return list;
        }

        public void Add(Model model)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
            @"INSERT INTO models (model_code, model_name)
              VALUES (@code, @name)";

            cmd.Parameters.AddWithValue("@code", model.ModelCode);
            cmd.Parameters.AddWithValue("@name", model.ModelName);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM models WHERE id = @id";

            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }


    }
}
