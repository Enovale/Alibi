using AO2Sharp.Helpers;
using AO2Sharp.Protocol;
using NetCoreServer;
using System;
using System.Net;
using System.Text;

namespace AO2Sharp
{
    internal class ClientSession : TcpSession
    {
        public Client Client { get; private set; }

        public ClientSession(TcpServer server) : base(server)
        {
        }

        protected override void OnConnected()
        {
            AO2Sharp.Server.Logger.Log(LogSeverity.Info, "Session connected: " + Socket.RemoteEndPoint, true);
            Client = new Client(Server as Server, this, IPAddress.Parse(((IPEndPoint)Socket.RemoteEndPoint).Address.ToString()));
            Client.LastAlive = DateTime.Now;

            // fuck fantaencrypt
            SendAsync(new AOPacket("decryptor", "NOENCRYPT"));
        }

        protected override void OnDisconnected()
        {
            AO2Sharp.Server.Logger.Log(LogSeverity.Info, "Session terminated.", true);

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
