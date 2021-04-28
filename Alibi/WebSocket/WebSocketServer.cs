using System.Net;
using System.Net.Sockets;
using Alibi.Plugins.API;
using NetCoreServer;

namespace Alibi.WebSocket
{
    internal class WebSocketProxy : WsServer
    {
        private readonly IServer _baseServer;
        
        public WebSocketProxy(IServer baseServer, IPAddress address, int port) : base(address, port)
        {
            _baseServer = baseServer;
        }

        protected override void OnError(SocketError error)
        {
            Server.Logger.Log(LogSeverity.Error, " " + error);
        }

        protected override TcpSession CreateSession()
        {
            return new WebSocketSession(_baseServer, this);
        }
    }
}