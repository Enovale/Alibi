#nullable enable
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
        /// TODO: Document what things are hot-reloaded, if not all
        /// </remarks>
        public void ReloadConfig();
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
    }
}