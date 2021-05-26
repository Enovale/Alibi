#nullable enable
using System;
using System.Collections.Generic;
using System.Net;

// ReSharper disable UnassignedGetOnlyAutoProperty
#pragma warning disable 8618

namespace Alibi.Plugins.API
{
    /// <summary>
    /// Represents the main server object.
    /// </summary>
    public interface IServer
    {
        public IConfiguration ServerConfiguration { get; }

        /// <summary>
        /// Resolved IP Address of the AO2 masterserver.
        /// </summary>
        /// <remarks>
        /// This IP is resolved from the domain specified in ServerConfiguration.MasterserverAddress
        /// </remarks>
        public IPAddress MasterServerIp { get; }

        /// <summary>
        /// An interface that allows you to interact with the database containing bans, mods, etc
        /// </summary>
        public IDatabaseManager Database { get; }

        /// <summary>
        /// List of music entries specified in the configuration.
        /// </summary>
        /// <remarks>
        /// Not every entry is a valid song that clients can play, so don't expect it to be.
        /// </remarks>
        public string[] MusicList { get; }

        /// <summary>
        /// List of characters specified in the configuration.
        /// </summary>
        /// <remarks>
        /// Duplicates are allowed, and will allow for two players being the same character.
        /// </remarks>
        public string[] CharactersList { get; }

        public string Version { get; }

        /// <summary>
        /// Collection of all clients connected to this server.
        /// </summary>
        /// <remarks>
        /// This isn't necessarily fully identified clients, so do sanity checking.
        /// Use either the client state of IClient.Connected
        /// </remarks>
        public List<IClient> ClientsConnected { get; }

        /// <summary>
        /// How many fully identified players are currently playing on the server?
        /// </summary>
        public int ConnectedPlayers { get; set; }

        /// <summary>
        /// Collection of areas on this server. Refer to IArea.
        /// </summary>
        public IArea[] Areas { get; }

        /// <summary>
        /// List of area names in the same order as Areas[]
        /// </summary>
        /// <remarks>
        /// TODO: Honestly I have no clue with this is here
        /// </remarks>
        public string[] AreaNames { get; }

        /// <summary>
        /// Should this server log and print logs that are marked as Verbose?
        /// </summary>
        public bool VerboseLogs { get; }

        /// <summary>
        /// Completely stop and destroy the server. Will end the server process, so be careful.
        /// </summary>
        /// <returns>TODO: idk</returns>
        public bool Stop();

        /// <summary>
        /// Hot-reloads the server configuration from disk.
        /// </summary>
        /// <remarks>
        /// This will reload your available characters, music, and the server configuration.
        /// It will also restart the server advertiser (what puts your server on the server list).
        /// Warning: This can break things depending on what you change, the server is not completely
        /// tailored to be compatible with this just yet.
        /// </remarks>
        public void ReloadConfig();

        /// <summary>
        /// Hot reloads less server configuration to prevent unexpected errors.
        /// </summary>
        /// <remarks>
        /// This reloads the music list and the characters list, and nothing else.
        /// </remarks>
        public void InitializeLists();

        /// <summary>
        /// Handles an arbitrary packet internally. Intended for plugins.
        /// </summary>
        /// <param name="client">The client which sent the packet</param>
        /// <param name="packet">The packet that was sent.</param>
        /// <remarks>
        /// This is handled identically to if a client were to send a packet over
        /// the socket to the server, so treat it as such.
        /// </remarks>
        public void HandlePacket(IClient client, AOPacket packet);

        /// <summary>
        /// Sends a packet to every client on the server.
        /// </summary>
        /// <param name="message">The packet to send.</param>
        public void Broadcast(AOPacket message);

        /// <summary>
        /// Sends a string to every client on the server
        /// that is displayed in their Out of Context chat
        /// </summary>
        /// <param name="message">The message to be sent</param>
        public void BroadcastOocMessage(string message);

        /// <summary>
        /// Find a client using an id, OoC name, character name, or HWID. (IPs dont work)
        /// </summary>
        /// <param name="str">An id, ooc name, char name, or HWID to search for.</param>
        /// <returns>The client that was found</returns>
        public IClient? FindUser(string str);

        /// <summary>
        /// Bans an IP address, whether it's online or not, and kicks any clients using it.
        /// </summary>
        /// <param name="ip">The IP to ban</param>
        /// <param name="reason">Why they were banned</param>
        /// <param name="expireDate">When their ban should be lifted, if ever</param>
        /// <param name="banner">The person that banned them (null if the server did it)</param>
        public void BanIp(IPAddress ip, string reason, TimeSpan? expireDate = null, IClient? banner = null);

        /// <summary>
        /// Bans an Hardware ID, whether it's online or not, and kicks any clients using it.
        /// </summary>
        /// <param name="hwid">The hardware ID to ban</param>
        /// <param name="reason">Why they were banned</param>
        /// <param name="expireDate">When their ban should be lifted, if ever</param>
        /// <param name="banner">The person that banned them (null if the server did it)</param>
        public void BanHwid(string hwid, string reason, TimeSpan? expireDate = null, IClient? banner = null);
    }
}