using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using NetCoreServer;

namespace AO2Sharp.WebSocket
{
    internal class WebSocketSession : WsSession
    {
        private TcpProxy _tcpSocket;

        public WebSocketSession(WsServer server) : base(server)
        {
            _tcpSocket = new TcpProxy(this, IPAddress.Loopback, AO2Sharp.Server.ServerConfiguration.Port);
        }

        public override void OnWsConnected(HttpRequest request)
        {
            _tcpSocket.ConnectAsync();
        }

        public override void OnWsDisconnected()
        {
            _tcpSocket.DisconnectAsync();
        }

        public override void OnWsError(string error)
        {
            Console.WriteLine(error);
        }

        public override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            _tcpSocket.SendAsync(buffer, offset, size);
        }
    }
}
