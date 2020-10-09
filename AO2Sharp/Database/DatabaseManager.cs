using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SQLite;

namespace AO2Sharp.Database
{
    public class DatabaseManager
    {
        public const string DatabaseFolder = "DatabaseManager";
        public static readonly string DatabasePath = Path.Combine(DatabaseFolder, "database.db");

        private SQLiteAsyncConnection _sql;

        public DatabaseManager()
        {
            _sql = new SQLiteAsyncConnection(DatabasePath);
        }
    }
}
