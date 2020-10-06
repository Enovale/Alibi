using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using NetCoreServer;

namespace AO2Sharp.WebSocket
{
    internal class WebSocketProxy : WsServer
    {
        public WebSocketProxy(IPAddress address, int port) : base(address, port)
        {
        }

        protected override TcpSession CreateSession()
        {
            return new WebSocketSession(this);
        }
    }
}
