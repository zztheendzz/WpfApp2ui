using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.model;

namespace WpfApp2.Services
{
    class ModelService
    {

            public List<Model> GetAll()
            {
                var list = new List<Model>();

                using var conn = DatabaseConfig.GetConnection();
                conn.Open();

                var cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT Id, ModelCode, ModelName FROM Model";

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new Model
                    {
                        Id = reader.GetInt32(0),
                        ModelCode = reader.GetString(1),
                        ModelName= reader.GetString(2),

                    });
                }

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
