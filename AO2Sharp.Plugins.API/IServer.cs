using System.Collections.Generic;
using System.Net;

namespace AO2Sharp.Plugins.API
{
    public interface IServer
    {
        public static IPAddress MasterServerIp { get; }
        public static string[] MusicList { get; }
        public static string[] CharactersList { get; }
        public static string Version { get; }

        public List<IClient> ClientsConnected { get; }

        public int ConnectedPlayers { get; set; }
        public IArea[] Areas { get; }
        public string[] AreaNames { get; }
        public bool VerboseLogs { get; }

        public bool Stop();
    }
}
