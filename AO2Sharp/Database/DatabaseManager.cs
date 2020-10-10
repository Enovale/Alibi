﻿using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace AO2Sharp.Database
{
    public class DatabaseManager
    {
        public const string DatabaseFolder = "Database";
        public static readonly string DatabasePath = Path.Combine(DatabaseFolder, "database.db");

        private SQLiteConnectionWithLock _sql;

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
                    PassHash = "$2y$11$Zz.qeWzmJPTMiS/IJi.1qeREWoHTavQji2lGC.xzWFuv4ceQgMP3y"
                });
        }

        public bool AddUser(string hdid, string ip)
        {
            var query = _sql.Table<User>().Where(u => u.Hdid == hdid);
            var list = query.ToArray();

            if (list.Length > 1)
            {
                Server.Logger.Log(LogSeverity.Error, "Two of the same hdid found, please edit the db...");
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

            var newUser = new User()
            {
                Banned = false,
                BanReason = "",
                Hdid = hdid,
                Ips = ip
            };

            _sql.InsertOrReplace(newUser);

            return true;
        }

        public void ChangeIp(string hdid, string oldIp, string newIp)
        {
            var user = _sql.Table<User>().First(u => u.Hdid == hdid);
            if (user.Ips.Contains(oldIp) && !user.Ips.Contains(newIp))
                user.Ips = user.Ips.Replace(oldIp, newIp);
            else if (user.Ips.Contains(oldIp))
                // Can probably be done better
                user.Ips = user.Ips.Replace(";" + oldIp, "").Replace(oldIp + ";", "").Replace(oldIp, "");
            else
                user.Ips += ";" + newIp;

            _sql.Update(user);
        }

        public string GetHdidfromIp(string ip)
        {
            return _sql.Table<User>().First(u => u.Ips.Contains(ip)).Hdid;
        }

        public bool IsHdidBanned(string hdid)
        {
            return _sql.Table<User>().Any(u => u.Banned && u.Hdid == hdid);
        }

        public bool IsIpBanned(string ip)
        {
            return IsHdidBanned(GetHdidfromIp(ip));
        }

        public string GetBanReason(string ip)
        {
            return _sql.Table<User>().First(u => u.Ips.Contains(ip)).BanReason;
        }

        public void BanHdid(string hdid, string reason)
        {
            var user = _sql.Table<User>().First(u => u.Hdid == hdid);
            user.Banned = true;
            user.BanReason = reason;

            _sql.Update(user);
        }

        public void BanIp(string ip, string reason)
        {
            BanHdid(GetHdidfromIp(ip), reason);
        }

        public void UnbanHdid(string hdid)
        {
            var user = _sql.Table<User>().First(u => u.Hdid == hdid);
            user.Banned = false;

            _sql.Update(user);
        }

        public void UnbanIp(string ip)
        {
            UnbanHdid(GetHdidfromIp(ip));
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
