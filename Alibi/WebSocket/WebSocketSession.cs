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
            _client = new Client((Server) _baseServer, this, ((IPEndPoint) Socket.RemoteEndPoint!).Address)
                {LastAlive = DateTime.UtcNow};
            _client?.OnSessionConnected();
        }

        public override void OnWsDisconnected()
        {
            _client?.OnSessionDisconnected();
        }

        protected override void OnDisconnected()
        {
            _client?.OnSessionDisconnected();
        }

        public override void OnWsError(string error)
        {
            Alibi.Server.Logger.Log(LogSeverity.Error, " " + error);
        }

        public override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            _client?.OnSessionReceived(buffer, offset, size);
        }

        public new long Send(string text)
        {
            return SendText(text);
        }

        public new bool SendAsync(string text)
        {
            return SendTextAsync(text);
        }
    }
}