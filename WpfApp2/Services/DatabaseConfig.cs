using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.Services
{
    public static class DatabaseConfig
    {
        private static string connectionString =
            "Data Source=DataBase/database.db";
        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection(connectionString);
        }
    }
}
