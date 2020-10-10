using AO2Sharp.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AO2Sharp
{
    // TODO: Maybe rewrite this using a TcpClient so we can better handle errors
    internal class Advertiser
    {
        private Socket Socket;

        public Advertiser(IPAddress ip, int port)
        {
            try
            {
                Socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Socket.BeginConnect(new IPEndPoint(ip, port), OnConnect, Socket);
            }
            catch (SocketException e)
            {
                Server.Logger.Log(LogSeverity.Error, "Advertiser disconnected: " + e.Message);
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            ((Socket)ar.AsyncState).EndConnect(ar);

            string ports;
            if (string.IsNullOrWhiteSpace(Server.ServerConfiguration.WebsocketPort.ToString()))
                ports = Server.ServerConfiguration.Port.ToString();
            else
                ports = Server.ServerConfiguration.Port + "&" + Server.ServerConfiguration.WebsocketPort;

            Socket.Send(Encoding.UTF8.GetBytes(new AOPacket("SCC", new[]
            {
                ports,
                Server.ServerConfiguration.ServerName,
                Server.ServerConfiguration.ServerDescription,
                $"AO2Sharp v{Server.Version}"
            })));
        }

        public void Stop()
        {
            Socket.Disconnect(true);
        }
    }
}
