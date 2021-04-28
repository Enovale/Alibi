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
            if (Alibi.Server.ServerConfiguration.Advertise &&
                ip.Equals(Alibi.Server.MasterServerIp))
                Alibi.Server.Logger.Log(LogSeverity.Info, " Probed by master server.", true);
            _client = new Client((Server) Server, this, ip)
                {LastAlive = DateTime.Now};
            _client.OnConnected();
        }

        protected override void OnDisconnected()
        {
            _client?.OnDisconnected();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            _client?.OnReceived(buffer, offset, size);
        }
    }
}