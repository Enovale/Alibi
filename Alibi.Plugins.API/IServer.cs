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
        public static IConfiguration ServerConfiguration { get; }
        public static IPAddress MasterServerIp { get; }
        public static IDatabaseManager Database { get; }
        public static string[] MusicList { get; }
        public static string[] CharactersList { get; }
        public static string Version { get; }

        public List<IClient> ClientsConnected { get; }

        public int ConnectedPlayers { get; set; }
        public IArea[] Areas { get; }
        public string[] AreaNames { get; }
        public bool VerboseLogs { get; }

        public bool Stop();
        public void ReloadConfig();
        public void Broadcast(IAOPacket message);
        public void BroadcastOocMessage(string message);

        /// <summary>
        ///     Find a client using an id, ooc name, character name, or hwid. (IPs dont work)
        /// </summary>
        /// <param name="str">an id, ooc name, char name, or HWID to search for.</param>
        /// <returns></returns>
        public IClient? FindUser(string str);

        public void OnAllPluginsLoaded();
        public void OnPlayerJoined(IClient client);
        public bool OnIcMessage(IClient client, ref string message);
        public bool OnOocMessage(IClient client, ref string message);
        public bool OnMusicChange(IClient client, ref string song);
        public bool OnModCall(IClient client, IAOPacket packet);
        public bool OnBan(IClient client, IClient banner, ref string reason, TimeSpan? expires = null);
    }
}