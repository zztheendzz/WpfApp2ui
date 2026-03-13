using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace WpfApp2.Services
{
    public class DatabaseService
    {
        private string connectionString = "Data Source=database.db";

        public IDbConnection GetConnection()
        {
            return new SqliteConnection(connectionString);
        }
    }
}
