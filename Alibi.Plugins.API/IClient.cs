#nullable enable
using System;
using System.Net;

namespace Alibi.Plugins.API
{
    public interface IClient
    {
        /// <summary>
        /// Quick reference to the main server.
        /// </summary>
        /// <remarks>
        /// Use this in your message/command handlers so you don't have to
        /// pass around references to the server.
        /// </remarks>
        public IServer ServerRef { get; }

        /// <summary>
        /// Whether or not this client has given us a handshake and identified.
        /// A connected client is in an area and is playing.
        /// </summary>
        public bool Connected { get; }

        /// <summary>
        /// The authentication level of this client.
        /// This is increased when calling /login
        /// and decreased when calling /logout
        /// </summary>
        public int Auth { get; }

        /// <summary>
        /// The last time the client gave us a keepalive packet.
        /// </summary>
        /// <remarks>
        /// Use this to determine if a client is idle and/or
        /// their network has been disconnected.
        /// </remarks>
        public DateTime LastAlive { get; }

        public IPAddress IpAddress { get; }

        /// <summary>
        /// The hardware ID of this player. Should be
        /// completely unique for every player, and is
        /// not reused across sessions or reconnects.
        /// </summary>
        public string? HardwareId { get; }

        /// <summary>
        /// Which area is this client currently in?
        /// </summary>
        /// <remarks>
        /// Can be null when the player hasn't done the handshake yet.
        /// </remarks>
        public IArea? Area { get; }

        /// <summary>
        /// The court position of this client, such as pro, def, or jud
        /// </summary>
        /// <remarks>
        /// Can be null if the client hasn't joined an area yet.
        /// </remarks>
        public string? Position { get; set; }

        /// <summary>
        /// The protocol-dependent state of this client. Use this to determine
        /// if a client is breaking protocol, or to tell if they're identified.
        /// </summary>
        public ClientState CurrentState { get; set; }

        /// <summary>
        /// The password this client input in order to unlock the character they're playing.
        /// </summary>
        public string? Password { get; }

        /// <summary>
        /// The character this player is currently playing as,
        /// as an index based on IServer.CharacterList.
        /// </summary>
        public int? Character { get; set; }

        /// <summary>
        /// The actual string name of the character this player is using.
        /// </summary>
        /// <remarks>
        /// This is arbitrary based on server configuration. Try not to hardcode anything.
        /// </remarks>
        public string? CharacterName { get; }

        /// <summary>
        /// The last name this player used when sending a message in Out of Context chat
        /// </summary>
        /// <remarks>
        /// Players are not forced to keep names and multiple players can have the same name.
        /// Do not use this for authentication or to identify a single player.
        /// </remarks>
        public string? OocName { get; }

        /// <summary>
        /// The last message this player has sent in the In-Character chat.
        /// </summary>
        public string? LastSentMessage { get; set; }

        /// <summary>
        /// Whether or not this player is able to speak.
        /// </summary>
        public bool Muted { get; set; }

        // Retarded pairing shit
        /// <summary>
        /// Which character (not player) this player is trying to pair with,
        /// as an index of IServer.CharacterList
        /// </summary>
        public int PairingWith { get; }

        /// <summary>
        /// The emote this player is trying to send in a pair.
        /// </summary>
        public string? StoredEmote { get; }

        /// <summary>
        /// The X offset this player is trying to use in a pair.
        /// </summary>
        public int StoredOffset { get; }

        /// <summary>
        /// Whether or not this player wants to flip in their pair message.
        /// </summary>
        public bool StoredFlip { get; }

        /// <summary>
        /// Changes the area of this client gracefully.
        /// </summary>
        /// <param name="index">Index of the area to switch to using IServer.Areas</param>
        public void ChangeArea(int index);

        /// <summary>
        /// Kick this player from the server, only if they area banned.
        /// </summary>
        /// <remarks>
        /// Shouldn't really run this often as it's already used internally
        /// </remarks>
        public void KickIfBanned();

        /// <summary>
        /// Kick a player from the server, using the reason specified.
        /// </summary>
        /// <param name="reason">The reason to give to the client as to why they were kicked.</param>
        public void Kick(string reason);

        /// <summary>
        /// Fetch the reason for why a client was banned from the database.
        /// </summary>
        /// <returns>Why the client was banned.</returns>
        public string GetBanReason();

        /// <summary>
        /// Ban this player's hardware ID, ensuring they don't rejoin under a proxy.
        /// </summary>
        /// <param name="reason">Why the client is being banned</param>
        /// <param name="expireDate">When their ban will expire (null for an indefinite ban)</param>
        /// <param name="banner">Who banned this client (null if the server banned them)</param>
        /// <remarks>
        /// If possible, this should always be used when banning, on top of IP banning.
        /// It ensures the player can't use a proxy, VPN, or a change of IP to bypass the ban.
        /// </remarks>
        public void BanHwid(string reason, TimeSpan? expireDate, IClient banner);

        /// <summary>
        /// Ban this player's IP address, which is easily changing.
        /// </summary>
        /// <param name="reason">Why the client is being banned</param>
        /// <param name="expireDate">When their ban will expire (null for an indefinite ban)</param>
        /// <param name="banner">Who banned this client (null if the server banned them)</param>
        /// <remarks>
        /// When trying to ban a player, this should almost always be accompanied by BanHwid, otherwise
        /// a player can simply change IPs using a VPN, Proxy, or a reset of their router to join the
        /// server again. However, if using this as a method of rate limiting, banning only their IP is acceptable.
        /// </remarks>
        public void BanIp(string reason, TimeSpan? expireDate, IClient banner);

        /// <summary>
        /// Send this client a packet.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        public void Send(IAOPacket packet);

        /// <summary>
        /// Send this player a message to be displayed in the Out of Context chat.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="sender">Who sent it? (null if the server sent it)</param>
        public void SendOocMessage(string message, string? sender = null);
    }
}