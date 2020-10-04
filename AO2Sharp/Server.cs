using System;
using System.Collections.Generic;
using System.Text;
using NetCoreServer;

namespace AO2Sharp
{
    internal class Server : TcpServer
    {
        public static Configuration ServerConfiguration;

        public Server(Configuration config) : base(config.BoundIpAddress, config.Port)
        {
            ServerConfiguration = config;
        }

        protected override TcpSession CreateSession()
        {
            return new ClientSession(this);
        }
    }
}
