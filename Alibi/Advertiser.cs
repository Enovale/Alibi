using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Alibi.Helpers;
using Alibi.Plugins.API;
using AOPacket = Alibi.Helpers.AOPacket;

namespace Alibi
{
    // TODO: Maybe rewrite this using a TcpClient so we can better handle errors
    internal class Advertiser
    {
        private readonly Socket _socket;

        public Advertiser(IPAddress ip, int port)
        {
            try
            {
                _socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _socket.BeginConnect(new IPEndPoint(ip, port), OnConnect, _socket);
            }
            catch (SocketException e)
            {
                Server.Logger.Log(LogSeverity.Error, " Advertiser disconnected: " + e);
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            ((Socket) ar.AsyncState)?.EndConnect(ar);

            string ports;
            if (string.IsNullOrWhiteSpace(Server.ServerConfiguration.WebsocketPort.ToString()))
                ports = Server.ServerConfiguration.Port.ToString();
            else
                ports = Server.ServerConfiguration.Port + "&" + Server.ServerConfiguration.WebsocketPort;

            _socket.Send(Encoding.UTF8.GetBytes(new AOPacket("SCC",
                ports,
                Server.ServerConfiguration.ServerName,
                Server.ServerConfiguration.ServerDescription,
                $"Alibi v{Server.Version}"
            )));
        }

        public void Stop()
        {
            _socket.Disconnect(true);
        }
    }
}