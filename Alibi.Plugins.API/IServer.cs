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

        public void OnAllPluginsLoaded();
        public bool OnIcMessage(IClient client, string message);
        public bool OnOocMessage(IClient client, string message);
        public bool OnMusicChange(IClient client, string song);
        public bool OnModCall(IClient client, IAOPacket packet);
        public bool OnBan(IClient client, string reason, TimeSpan? expires = null);
    }
}
