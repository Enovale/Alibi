using AO2Sharp.Helpers;
using AO2Sharp.Protocol;
using NetCoreServer;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
            var ip = ((IPEndPoint) Socket.RemoteEndPoint).Address;
            if(AO2Sharp.Server.ServerConfiguration.Advertise && ip.Equals(AO2Sharp.Server.MasterServerIp))
                AO2Sharp.Server.Logger.Log(LogSeverity.Info, " Probed by master server.", true);
            else
                AO2Sharp.Server.Logger.Log(LogSeverity.Info, " Session connected: " + Socket.RemoteEndPoint, true);
            Client = new Client(Server as Server, this, ip);
            Client.LastAlive = DateTime.Now;

            // fuck fantaencrypt
            SendAsync(new AOPacket("decryptor", "NOENCRYPT"));
        }

        protected override void OnDisconnected()
        {
            AO2Sharp.Server.Logger.Log(LogSeverity.Info, " Session terminated.", true);

            ((Server)Server).ClientsConnected.Remove(Client);
            if (Client.Connected)
            {
                ((Server)Server).ConnectedPlayers--;
                Client.Area.PlayerCount--;
                if (Client.Character != null)
                    Client.Area.TakenCharacters[(int)Client.Character] = false;
                Client.Area.UpdateTakenCharacters();
                Client.Area.AreaUpdate(AreaUpdateType.PlayerCount);
            }
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string msg = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            string[] packets = msg.Split("%", StringSplitOptions.RemoveEmptyEntries);
            foreach (var packet in packets)
            {
                MessageHandler.HandleMessage(Client, AOPacket.FromMessage(packet));
            }
            Client.LastAlive = DateTime.Now;
        }
    }
}
