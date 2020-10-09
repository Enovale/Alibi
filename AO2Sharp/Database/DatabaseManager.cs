using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SQLite;

namespace AO2Sharp.Database
{
    public class DatabaseManager
    {
        public const string DatabaseFolder = "Database";
        public static readonly string DatabasePath = Path.Combine(DatabaseFolder, "database.db");

        private SQLiteConnection _sql;

        public DatabaseManager()
        {
            if (!Directory.Exists(DatabaseFolder))
                Directory.CreateDirectory(DatabaseFolder);
            if(!File.Exists(DatabasePath))
                File.Create(DatabasePath).Close();
            _sql = new SQLiteConnection(DatabasePath);
            _sql.CreateTable<User>();
            _sql.CreateTable<Login>();
        }

        public bool AddUser(string hwid, string ip)
        {
            var query = _sql.Table<User>().Where(u => u.Hdid == hwid);
            var list = query.ToArray();

            if (list.Length > 1)
            {
                Server.Logger.Log(LogSeverity.Error, "Two of the same hdid found, please edit the db...");
                return false;
            }

            if (list.Length == 1)
            {
                string[] ips = list.First().Ips.Split(";");
                if (!ips.Contains(ip))
                {
                    list.First().Ips += ip + ";";
                    return true;
                }
                return false;
            }

            var newUser = new User()
            {
                Banned = false,
                BanReason = "",
                Hdid = hwid,
                Ips = ip
            };

            _sql.Insert(newUser);

            return true;
        }
    }
}
