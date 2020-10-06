using AO2Sharp.Helpers;
using NetCoreServer;
using System.Net;

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
            IPAddress ip = (_session.Socket.RemoteEndPoint as IPEndPoint)?.Address;
            if (IPAddress.IsLoopback(ip))
                Send(new AOPacket("WSIP", ip.ToString()));
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
