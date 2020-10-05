using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using NetCoreServer;

namespace AO2Sharp
{
    internal class Server : TcpServer
    {
        public static Configuration ServerConfiguration;
        public static string Version;

        public readonly List<Client> ClientsConnected;
        public int ConnectedPlayers = 0;

        private Advertiser _advertiser;

        public Server(Configuration config) : base(config.BoundIpAddress, config.Port)
        {
            ServerConfiguration = config;
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Version = fileVersionInfo.ProductVersion;

            ClientsConnected = new List<Client>(ServerConfiguration.MaxPlayers);
            _advertiser = new Advertiser(ServerConfiguration.MasterServerAddress, ServerConfiguration.MasterServerPort);
        }

        protected override TcpSession CreateSession()
        {
            return new ClientSession(this);
        }
    }
}
