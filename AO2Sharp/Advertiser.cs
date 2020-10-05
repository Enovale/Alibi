using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AO2Sharp.Helpers;
using NetCoreServer;

namespace AO2Sharp
{
    // TODO: Maybe rewrite this using a TcpClient so we can better handle errors
    internal class Advertiser
    {
        private Socket Socket;

        public Advertiser(string masterServer, int port)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(masterServer);
            IPAddress ipAddress = ipHostInfo.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

            try
            {
                Socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Socket.BeginConnect(new IPEndPoint(ipAddress, port), OnConnect, Socket);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Advertiser disconnected: " + e.Message);
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
    }
}
