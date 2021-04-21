using System;

namespace Alibi.Plugins.API
{
    /// <summary>
    /// Provides an interface to interact with the internal database. Be careful.
    /// </summary>
    public interface IDatabaseManager
    {
        /// <summary>
        /// Add a user to the database, with default info.
        /// </summary>
        /// <param name="hwid">The hardware ID of this user</param>
        /// <param name="ip">The IP Address of this user</param>
        /// <returns>Whether or not the user was successfully added to the database.</returns>
        public bool AddUser(string hwid, string ip);

        /// <summary>
        /// Get all hardware IDs associated with a given IP Address
        /// </summary>
        /// <param name="ip">The IP Address to search the database for</param>
        /// <returns>An array of hardware IDs associated with this IP</returns>
        public string[] GetHwidsfromIp(string ip);

        /// <summary>
        /// Check if a given hardware ID is banned in the database.
        /// </summary>
        /// <param name="hwid">The hardware ID to search for</param>
        /// <returns>Whether or not this HWID is banned</returns>
        public bool IsHwidBanned(string hwid);

        /// <summary>
        /// Check if a given IP Address is banned in the database.
        /// </summary>
        /// <param name="ip">The IP Address to check</param>
        /// <returns>Whether or not this IP Address is banned</returns>
        public bool IsIpBanned(string ip);

        /// <summary>
        /// Gets the ban reasoning stored in the database for an IP Address.
        /// </summary>
        /// <param name="ip">The IP Address to search for</param>
        /// <returns>The reason this IP was banned</returns>
        /// <remarks>
        /// This isn't available for HWIDs for technical reasons.
        /// </remarks>
        public string GetBanReason(string ip);

        /// <summary>
        /// Registers a ban for an HWID in the database. This won't take effect
        /// until the player rejoins the server. Instead use IClient.BanHwid
        /// </summary>
        /// <param name="hwid">The HWID to ban</param>
        /// <param name="reason">Why they were banned</param>
        /// <param name="expireTime">When to let this ban expire (null if indefinite)</param>
        public void BanHwid(string hwid, string reason, TimeSpan? expireTime = null);

        /// <summary>
        /// Registers a ban for an IP in the database. This won't take effect
        /// until the player rejoins the server. Instead use IClient.BanIP
        /// </summary>
        /// <param name="ip">The IP Address to ban</param>
        /// <param name="reason">Why they are being banned</param>
        /// <param name="expireTime">When to let this  ban expire (null if indefinite)</param>
        /// <remarks>
        /// Try to never use this ban method as it is insecure. Use HWID banning whenever possible.
        /// </remarks>
        public void BanIp(string ip, string reason, TimeSpan? expireTime = null);

        /// <summary>
        /// Lifts a ban for an HWID in the database.
        /// </summary>
        /// <param name="hwid">The Hardware ID to unban</param>
        public void UnbanHwid(string hwid);

        /// <summary>
        /// Lifts a ban for an IP Address in the database.
        /// </summary>
        /// <param name="ip">The IP Address to unban</param>
        public void UnbanIp(string ip);

        /// <summary>
        /// Gets a list of every banned Hardware ID in the database.
        /// </summary>
        /// <returns>An array of HWIDs that are banned in the database</returns>
        public string[] GetBannedHwids();

        /// <summary>
        /// Fetches when an HWID will be unbanned.
        /// </summary>
        /// <param name="hwid">The HWID to search for</param>
        /// <returns>A DateTime representing when this ban will expire.</returns>
        public DateTime? GetBanExpiration(string hwid);

        /// <summary>
        /// Registers an account with higher permissions in the database.
        /// </summary>
        /// <param name="username">The username this player will use to login</param>
        /// <param name="password">The password this player will use to login</param>
        /// <param name="perms">What permissions this account has, refer to AuthType</param>
        /// <returns>Whether or not this operation was successful</returns>
        /// <remarks>
        /// Passwords are stored as a BCrypt hash in the database
        /// and the plain text version is immediately thrown away.
        /// </remarks>
        public bool AddLogin(string username, string password, int perms);

        /// <summary>
        /// Change the permission level of a given login in the database.
        /// </summary>
        /// <param name="username">The login username to change permissions of</param>
        /// <param name="perms">The permissions to change the login to</param>
        /// <returns>Whether or not this operation was successful</returns>
        public bool ChangeLoginPermissions(string username, int perms);

        /// <summary>
        /// Deletes a login registered in the database.
        /// </summary>
        /// <param name="username">The login to delete</param>
        /// <returns>Whether or not this operation was successful</returns>
        /// <remarks>
        /// Doing this will NOT log out players that already used this login.
        /// Do not use this as a token revocation system of sorts, unless you
        /// also find those logged in users and log them out.
        /// </remarks>
        public bool RemoveLogin(string username);

        /// <summary>
        /// Checks if the given username and password matches the one registered in the database.
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <param name="password">The password to check (in plaintext)</param>
        /// <returns>Whether or not the authentication was successful</returns>
        public bool CheckCredentials(string username, string password);

        /// <summary>
        /// Returns the permission level of a given login.
        /// </summary>
        /// <param name="username">The login to check</param>
        /// <returns>What permission level they have</returns>
        public int GetPermissionLevel(string username);
    }
}