using LiveCharts.Configurations;
using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.model;
using Dapper;
namespace WpfApp2.Services
{


    public class ModelService
    {
        private readonly DatabaseService _db = new DatabaseService();

  
        public void Delete(int id)
        {
            using var conn = _db.GetConnection();
            conn.Execute("DELETE FROM Model WHERE Id=@Id", new { Id = id });
        }
    }
}
