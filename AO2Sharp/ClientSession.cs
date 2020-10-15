using AO2Sharp.Helpers;
using AO2Sharp.Protocol;
using NetCoreServer;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AO2Sharp
{
    public class ClientSession : TcpSession
    {
        public Client Client { get; private set; }

        public ClientSession(TcpServer server) : base(server)
        {
        }

        protected override void OnConnected()
        {
            if (((Server) Server).ConnectedPlayers >= AO2Sharp.Server.ServerConfiguration.MaxPlayers)
            {
                Send(new AOPacket("BD", "Max players has been reached."));
                Task.Delay(500);
                Disconnect();
            }

            var ip = ((IPEndPoint)Socket.RemoteEndPoint).Address;
            if (AO2Sharp.Server.ServerConfiguration.Advertise && ip.Equals(AO2Sharp.Server.MasterServerIp))
                AO2Sharp.Server.Logger.Log(LogSeverity.Info, " Probed by master server.", true);
            Client = new Client(Server as Server, this, ip);
            Client.LastAlive = DateTime.Now;

            // fuck fantaencrypt
            SendAsync(new AOPacket("decryptor", "NOENCRYPT"));
        }

        protected override void OnDisconnected()
        {
            ((Server)Server).ClientsConnected.Remove(Client);
            if (Client.Connected)
            {
                ((Server)Server).ConnectedPlayers--;
                Client.Area!.PlayerCount--;
                Client.Connected = false;
                if (Client.Character != null)
                    Client.Area.TakenCharacters[(int)Client.Character] = false;
                Client.Area.UpdateTakenCharacters();
                Client.Area.CurrentCaseManagers.Remove(Client);
                Client.Area.AreaUpdate(AreaUpdateType.PlayerCount);
                Client.Area.AreaUpdate(AreaUpdateType.CourtManager);
            }
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string msg = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            string[] dissallowedRequests = "GET;HEAD;POST;PUT;DELETE;TRACE;OPTIONS;CONNECT;PATCH".Split(';');
            if (dissallowedRequests.Any(r => msg.StartsWith(r)))
                return;
            string[] packets = msg.Split("%", StringSplitOptions.RemoveEmptyEntries);
            foreach (var packet in packets)
            {
                MessageHandler.HandleMessage(Client, AOPacket.FromMessage(packet));
            }
            Client.LastAlive = DateTime.Now;
        }
    }
}
