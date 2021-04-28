#nullable enable
using System;
using System.Net;
using Alibi.Plugins.API;
using NetCoreServer;

namespace Alibi.WebSocket
{
    internal class WebSocketSession : WsSession, ISession
    {
        private Client? _client;
        private readonly IServer _baseServer;
        
        public WebSocketSession(IServer baseServer, WsServer server) : base(server)
        {
            OptionReceiveBufferSize = 65536;
            OptionSendBufferSize = 65536;
            _baseServer = baseServer;
        }

        public override void OnWsConnected(HttpRequest request)
        {
            var ip = ((IPEndPoint)Socket.RemoteEndPoint!).Address;
            Alibi.Server.Logger.Log(LogSeverity.Info,
                $"[{ip}] Websocket connection.", true);
            _client = new Client((Server) _baseServer, this, ip)
                {LastAlive = DateTime.Now};
            _client.OnConnected();
        }

        public override void OnWsDisconnected()
        {
            _client?.OnDisconnected();
        }

        protected override void OnDisconnected()
        {
            _client?.OnDisconnected();
        }

        public override void OnWsError(string error)
        {
            Alibi.Server.Logger.Log(LogSeverity.Error, " " + error);
        }

        public override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            _client?.OnReceived(buffer, offset, size);
        }
    }
}