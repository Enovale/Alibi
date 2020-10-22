using System;

namespace Alibi.Plugins.API
{
    public interface IDatabaseManager
    {
        public bool AddUser(string hwid, string ip);
        public string[] GetHwidsfromIp(string ip);
        public bool IsHwidBanned(string hwid);
        public bool IsIpBanned(string ip);
        public string GetBanReason(string ip);
        public void BanHwid(string hwid, string reason, TimeSpan? expireTime = null);
        public void BanIp(string ip, string reason, TimeSpan? expireTime = null);
        public void UnbanHwid(string hwid);
        public void UnbanIp(string ip);
        public string[] GetBannedHwids();
        public DateTime? GetBanExpiration(string hwid);
        public bool AddLogin(string username, string password, int perms);
        public bool ChangeLoginPermissions(string username, int perms);
        public bool RemoveLogin(string username);
        public bool CheckCredentials(string username, string password);
        public int GetPermissionLevel(string username);
    }
}