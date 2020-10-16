#nullable enable
using System;
using System.Collections.Generic;
using System.Net;

namespace Alibi.Plugins.API
{
    public interface IServer
    {
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
        public IClient? FindUser(string str);
        public bool AddUser(IClient client);
        public bool CheckLogin(string username, string password);
        public bool AddLogin(string username, string password);
        public bool RemoveLogin(string username);

        public void OnAllPluginsLoaded();
        public void OnModCall(IClient client, IAOPacket packet);
        public void OnBan(IClient client, string reason, TimeSpan? expires = null);
    }
}
