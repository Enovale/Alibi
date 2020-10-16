using Alibi.Plugins.API;
using NetCoreServer;
using System.Net;

namespace Alibi.WebSocket
{
    internal class WebSocketSession : WsSession
    {
        private readonly TcpProxy _tcpSocket;

        public WebSocketSession(WsServer server) : base(server)
        {
            _tcpSocket = new TcpProxy(this, IPAddress.Loopback, Alibi.Server.ServerConfiguration.Port);
        }

        public override void OnWsConnected(HttpRequest request)
        {
            Alibi.Server.Logger.Log(LogSeverity.Info, $"[{((IPEndPoint)Socket.RemoteEndPoint).Address}] Websocket connection.", true);
            _tcpSocket.ConnectAsync();
        }

        public override void OnWsDisconnected()
        {
            _tcpSocket.DisconnectAsync();
        }

        protected override void OnDisconnected()
        {
            _tcpSocket.DisconnectAsync();
        }

        public override void OnWsError(string error)
        {
            Alibi.Server.Logger.Log(LogSeverity.Error, " " + error, true);
        }

        public override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            _tcpSocket.SendAsync(buffer, offset, size);
        }
    }
}
