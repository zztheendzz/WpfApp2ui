using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace WpfApp2.Services
{
    public class DatabaseService
    {
        private string connectionString =
        @"Data Source=\\10.82.0.250\Automation\DataBase\Automation team Price Management System\database.db;Pooling=True;Default Timeout=5";

        public IDbConnection GetConnection()
        {
            var connection = new SqliteConnection(connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "PRAGMA journal_mode=WAL;";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "PRAGMA synchronous=NORMAL;";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "PRAGMA busy_timeout=5000;";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "PRAGMA temp_store=MEMORY;";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "PRAGMA cache_size=10000;";
            cmd.ExecuteNonQuery();

            return connection;
        }
    }
}