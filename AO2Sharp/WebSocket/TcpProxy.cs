using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
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

        public override long Send(string text)
        {
            return base.Send(text);
        }

        public override bool SendAsync(string text)
        {
            return base.SendAsync(text);
        }

        protected override void OnConnected()
        {
            base.OnConnected();
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            _session.SendAsync(buffer, offset, size);
        }
    }
}
