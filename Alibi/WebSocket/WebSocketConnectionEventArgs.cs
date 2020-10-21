using System;
using System.Net;

namespace Alibi.WebSocket
{
    internal class WebSocketConnectionEventArgs : EventArgs
    {
        public WebSocketConnectionEventArgs(TcpProxy tcpProxy, IPEndPoint remoteEndPoint)
        {
            TcpProxy = tcpProxy;
            RemoteEndPoint = remoteEndPoint;
        }

        public TcpProxy TcpProxy { get; }
        public IPEndPoint RemoteEndPoint { get; }
    }
}