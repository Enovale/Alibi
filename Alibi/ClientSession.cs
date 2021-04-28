#nullable enable
using System;
using System.Net;
using Alibi.Plugins.API;
using NetCoreServer;

namespace Alibi
{
    public class ClientSession : TcpSession, ISession
    {
        private Client? _client;

        public ClientSession(TcpServer server) : base(server)
        {
        }

        protected override void OnConnected()
        {
            var ip = ((IPEndPoint) Socket.RemoteEndPoint!).Address;
            if (((Server)Server).ServerConfiguration.Advertise &&
                ip.Equals(((Server)Server).MasterServerIp))
                Alibi.Server.Logger.Log(LogSeverity.Info, " Probed by master server.", true);
            _client = new Client((Server) Server, this, ip)
                {LastAlive = DateTime.Now};
            _client.OnSessionConnected();
        }

        protected override void OnDisconnected()
        {
            _client?.OnSessionDisconnected();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            _client?.OnSessionReceived(buffer, offset, size);
        }
    }
}