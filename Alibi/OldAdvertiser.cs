using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Alibi.Plugins.API;

namespace Alibi
{
    // TODO: Maybe rewrite this using a TcpClient so we can better handle errors
    internal class OldAdvertiser : IAdvertiser
    {
        private Socket _socket;

        public void Start(string url)
        {
            if (_socket != null)
                return;
            
            try
            {
                var uri = new Uri(url);
                var ip = Dns.GetHostAddresses(url).First();
                _socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _socket.BeginConnect(new IPEndPoint(ip, uri.Port), OnConnect, _socket);
            }
            catch (SocketException e)
            {
                Server.Logger.Log(LogSeverity.Error, " Advertiser disconnected: " + e);
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            var server = Server.Instance;
            ((Socket) ar.AsyncState)?.EndConnect(ar);

            string ports;
            if (string.IsNullOrWhiteSpace(server.ServerConfiguration.WebsocketPort.ToString()))
                ports = server.ServerConfiguration.Port.ToString();
            else
                ports = server.ServerConfiguration.Port + "&" + server.ServerConfiguration.WebsocketPort;

            _socket.Send(Encoding.UTF8.GetBytes(new AOPacket("SCC",
                ports,
                AOPacket.EncodeToAOPacket(server.ServerConfiguration.ServerName),
                AOPacket.EncodeToAOPacket(server.ServerConfiguration.ServerDescription),
                $"Alibi v{server.Version}"
            )));
        }

        public void Stop()
        {
            if (_socket == null)
                return;
            
            _socket.Disconnect(false);
            _socket.Dispose();
            _socket = null;
        }
    }
}