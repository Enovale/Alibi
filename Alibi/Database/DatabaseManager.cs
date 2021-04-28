using System;
using System.IO;
using System.Linq;
using Alibi.Plugins.API;
using SQLite;

namespace Alibi.Database
{
    public class DatabaseManager : IDatabaseManager
    {
        public const string DatabaseFolder = "Database";
        public static readonly string DatabasePath = Path.Combine(DatabaseFolder, "database.db");

        private readonly SQLiteConnectionWithLock _sql;

        public DatabaseManager()
        {
            if (!Directory.Exists(DatabaseFolder))
                Directory.CreateDirectory(DatabaseFolder);
            if (!File.Exists(DatabasePath))
                File.Create(DatabasePath).Close();
            _sql = new SQLiteConnectionWithLock(new SQLiteConnectionString(DatabasePath));
            _sql.Lock();
            _sql.CreateTable<User>();
            _sql.CreateTable<Login>();

            if (!_sql.Table<Login>().Any())
                _sql.Insert(new Login
                {
                    UserName = "admin",
                    // BCrypt Hashed version of 'ChangeThis'
                    PassHash = "$2y$11$Zz.qeWzmJPTMiS/IJi.1qeREWoHTavQji2lGC.xzWFuv4ceQgMP3y",
                    PermissionsLevel = AuthType.ADMINISTRATOR
                });
        }

        public bool AddUser(string hwid, string ip)
        {
            var query = _sql.Table<User>().Where(u => u.Hwid == hwid);
            var list = query.ToArray();

            if (list.Length > 1)
            {
                Server.Logger.Log(LogSeverity.Error, " Two of the same hwid found, please edit the db...");
                return false;
            }

            if (list.Length == 1)
            {
                var existingUser = list.First();
                if (!existingUser.Ips.Contains(ip))
                {
                    existingUser.Ips += ";" + ip;
                    _sql.Update(existingUser);
                    return true;
                }

                return false;
            }

            var newUser = new User
            {
                Banned = false,
                BanReason = "",
                Hwid = hwid,
                Ips = ip
            };

            _sql.InsertOrReplace(newUser);

            return true;
        }

        public string[] GetHwidsfromIp(string ip)
        {
            return _sql.Table<User>().Where(u => u.Ips.Contains(ip)).Select(u => u.Hwid).ToArray();
        }

        public bool IsHwidBanned(string hwid)
        {
            return _sql.Table<User>().Any(u => u.Banned && u.Hwid == hwid);
        }

        public bool IsIpBanned(string ip)
        {
            foreach (var hwid in GetHwidsfromIp(ip))
                if (IsHwidBanned(hwid))
                    return true;

            return false;
        }

        public string GetBanReason(string ip) => _sql.Table<User>().First(u => u.Ips.Contains(ip)).BanReason;

        public void BanHwid(string hwid, string reason, TimeSpan? expireTime = null)
        {
            var user = _sql.Table<User>().First(u => u.Hwid == hwid);
            user.Banned = true;
            user.BanReason = reason;
            if (expireTime != null && expireTime.Value == TimeSpan.Zero && user.BanExpiration == null)
                user.BanExpiration = DateTime.Now.Add((TimeSpan) expireTime);

            _sql.Update(user);
        }

        public void BanIp(string ip, string reason, TimeSpan? expireTime = null)
        {
            foreach (var hwid in GetHwidsfromIp(ip))
                BanHwid(hwid, reason, expireTime);
        }

        public void UnbanHwid(string hwid)
        {
            var user = _sql.Table<User>().First(u => u.Hwid == hwid);
            user.Banned = false;

            _sql.Update(user);
        }

        public void UnbanIp(string ip)
        {
            foreach (var hwid in GetHwidsfromIp(ip))
                UnbanHwid(hwid);
        }

        public string[] GetBannedHwids() => _sql.Table<User>().Where(u => u.Banned).Select(u => u.Hwid).ToArray();

        public DateTime? GetBanExpiration(string hwid) => _sql.Table<User>().Single(u => u.Hwid == hwid).BanExpiration;

        public bool AddLogin(string username, string password, int perms)
        {
            if (_sql.Table<Login>().Any(l => l.UserName.ToLower() == username.ToLower()))
                return false;

            var newLogin = new Login
            {
                UserName = username,
                PassHash = BCrypt.Net.BCrypt.HashPassword(password),
                PermissionsLevel = perms
            };
            _sql.Insert(newLogin);

            return true;
        }

        public bool ChangeLoginPermissions(string username, int perms)
        {
            var login = _sql.Table<Login>().FirstOrDefault(l => l.UserName.ToLower() == username.ToLower());
            if (login == null)
                return false;
            login.PermissionsLevel = perms;

            _sql.Update(login);
            return true;
        }

        public bool RemoveLogin(string username)
        {
            // Can't remove the last admin user.
            if (_sql.Table<Login>().Count(l => l.PermissionsLevel == AuthType.ADMINISTRATOR) == 1)
                return false;
            
            if (_sql.Delete<Login>(username) == 0)
                return false;

            return true;
        }

        public bool CheckCredentials(string username, string password)
        {
            var logins = _sql.Table<Login>().Where(l => l.UserName == username).ToArray();

            if (logins.Length <= 0)
                return false;

            var hash = logins.First().PassHash;
            if (BCrypt.Net.BCrypt.Verify(password, hash))
                return true;

            return false;
        }

        public int GetPermissionLevel(string username)
        {
            return _sql.Table<Login>()
                .FirstOrDefault(l => 
                    l.UserName.ToLower() == username.ToLower()).PermissionsLevel;
        }
    }
}