using System;
using System.Net;
using System.Text;
using NetCoreServer;

namespace Alibi.WebSocket
{
    internal class TcpProxy : TcpClient
    {
        private readonly WebSocketSession _session;


        public TcpProxy(WebSocketSession wsSession, IPAddress address, int port) : base(address, port)
        {
            _session = wsSession;
        }

        protected override void OnConnected()
        {
            if (_session.IsDisposed || _session.IsSocketDisposed) 
                Disconnect();
        }

        protected override void OnDisconnected()
        {
            _session.Disconnect();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            if (!_session.IsConnected || _session.IsDisposed || _session.IsSocketDisposed)
            {
                Disconnect();
                return;
            }

            var msg = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
            var packets = msg.Split("%", StringSplitOptions.RemoveEmptyEntries);
            foreach (var packet in packets)
            {
                try
                {
                    _session.SendTextAsync(packet);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}