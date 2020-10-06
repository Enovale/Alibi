using System;
using System.Collections.Generic;
using System.Text;
using NetCoreServer;

namespace AO2Sharp.WebSocket
{
    internal class WebSocketSession : WsSession
    {
        private TcpProxy _tcpSocket;

        public WebSocketSession(WsServer server) : base(server)
        {
            _tcpSocket = new TcpProxy(this, server.Endpoint.Address, AO2Sharp.Server.ServerConfiguration.Port);
            _tcpSocket.ConnectAsync();
        }

        public override long Send(string text)
        {
            return _tcpSocket.Send(text);
        }

        public override bool SendAsync(string text)
        {
            return _tcpSocket.SendAsync(text);
        }
    }
}
