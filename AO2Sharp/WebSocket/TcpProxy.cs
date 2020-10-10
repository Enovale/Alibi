using AO2Sharp.Helpers;
using NetCoreServer;
using System;
using System.Net;
using System.Text;

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
            if (_session.IsDisposed || _session.IsSocketDisposed)
            {
                Disconnect();
            }
        }

        protected override void OnDisconnected()
        {
            _session.Disconnect();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            if (_session.IsDisposed || _session.IsSocketDisposed)
            {
                Disconnect();
                return;
            }
            string msg = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            string[] packets = msg.Split("%", StringSplitOptions.RemoveEmptyEntries);
            foreach (var packet in packets)
            {
                if (packet.StartsWith("ID"))
                {
                    IPAddress ip = ((IPEndPoint)_session.Socket.RemoteEndPoint).Address;
                    Send(new AOPacket("WSIP", ip.ToString()));
                }
                _session.SendTextAsync(packet);
            }
        }
    }
}
