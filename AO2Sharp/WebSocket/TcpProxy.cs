using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using AO2Sharp.Helpers;
using NetCoreServer;

namespace AO2Sharp.WebSocket
{
    internal class TcpProxy : TcpClient
    {
        private WebSocketSession _session;

        public TcpProxy(WebSocketSession wsSession, IPAddress address, int port) : base(address, port)
        {
            _session = wsSession;
        }

        protected override void OnConnected()
        {
            Send(new AOPacket("WSIP", (_session.Socket.RemoteEndPoint as IPEndPoint)?.Address.ToString()));
        }

        protected override void OnDisconnected()
        {
            _session.Disconnect();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            _session.SendTextAsync(buffer, offset, size);
        }
    }
}
