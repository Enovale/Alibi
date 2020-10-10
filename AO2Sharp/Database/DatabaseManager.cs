using SQLite;
using System;
using System.IO;
using System.Linq;

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
            if (!File.Exists(DatabasePath))
                File.Create(DatabasePath).Close();
            _sql = new SQLiteConnection(DatabasePath);
            _sql.CreateTable<User>();
            _sql.CreateTable<Login>();

            if (!_sql.Table<Login>().Any())
                _sql.Insert(new Login
                {
                    UserName = "admin",
                    PassHash = "$2y$11$Zz.qeWzmJPTMiS/IJi.1qeREWoHTavQji2lGC.xzWFuv4ceQgMP3y"
                });
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

        public bool AddLogin(string username, string password)
        {
            if (_sql.Table<Login>().Any(l => l.UserName.ToLower() == username.ToLower()))
                return false;

            var newLogin = new Login()
            {
                UserName = username,
                PassHash = BCrypt.Net.BCrypt.HashPassword(password)
            };
            _sql.Insert(newLogin);

            return true;
        }

        public bool RemoveLogin(string username)
        {
            if (_sql.Delete<Login>(username) == 0)
                return false;

            return true;
        }

        public bool CheckCredentials(string username, string password)
        {
            var logins = _sql.Table<Login>().Where(l => l.UserName == username).ToArray();

            if (logins.Length <= 0)
                return false;

            string hash = logins.First().PassHash;
            if (BCrypt.Net.BCrypt.Verify(password, hash))
                return true;

            return false;
        }
    }
}
