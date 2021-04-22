using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Alibi.Helpers;
using Alibi.Plugins.API;
using Alibi.Protocol;
using Alibi.WebSocket;
using NetCoreServer;
using AOPacket = Alibi.Helpers.AOPacket;

namespace Alibi
{
    public class ClientSession : TcpSession
    {
        public Client Client { get; private set; }
        
        private DateTime _banCheckTime;
        private string _lastSentPacket;
        private int _packetCount;

        public ClientSession(TcpServer server) : base(server)
        {
        }

        protected override void OnConnected()
        {
            if (((Server) Server).ConnectedPlayers >= Alibi.Server.ServerConfiguration.MaxPlayers)
            {
                Send(new AOPacket("BD", "Not a real ban: Max players has been reached."));
                Task.Delay(500);
                Disconnect();
                return;
            }

            _banCheckTime = DateTime.Now;

            var ip = ((IPEndPoint) Socket.RemoteEndPoint).Address;

            if (IPAddress.IsLoopback(ip))
            {
                var result = WebSocketSession.AwaitingEndPoints.TryDequeue(out var ev);

                if (result)
                    ip = ev.RemoteEndPoint.Address;
            }

            if (Alibi.Server.ServerConfiguration.Advertise && ip.Equals(Alibi.Server.MasterServerIp))
                Alibi.Server.Logger.Log(LogSeverity.Info, " Probed by master server.", true);
            if (!IPAddress.IsLoopback(ip) &&
                ((Server) Server).ClientsConnected.Count(c => ip.ToString() == c.IpAddress.ToString())
                >= Alibi.Server.ServerConfiguration.MaxMultiClients)
            {
                Send(new AOPacket("BD", "Not a real ban: Can't have more than " +
                                        $"{Alibi.Server.ServerConfiguration.MaxMultiClients} clients at a time."));
                Task.Delay(500);
                Disconnect();
                return;
            }

            Client = new Client((Server) Server, this, ip) {LastAlive = DateTime.Now};
            Client.KickIfBanned();

            ((Server) Server).OnPlayerJoined(Client);

            // fuck fantaencrypt
            Send(new AOPacket("decryptor", "NOENCRYPT"));
        }

        protected override void OnDisconnected()
        {
            ((Server) Server).ClientsConnected.Remove(Client);
            if (Client != null && Client.Connected)
            {
                ((Server) Server).ConnectedPlayers--;
                ((Area) Client.Area)!.PlayerCount--;
                Client.Connected = false;
                if (Client.Character != null)
                    Client.Area.TakenCharacters[(int) Client.Character] = false;
                Client.Area.UpdateTakenCharacters();
                Client.Area.CurrentCaseManagers.Remove(Client);
                Client.Area.AreaUpdate(AreaUpdateType.PlayerCount);
                Client.Area.AreaUpdate(AreaUpdateType.CourtManager);
            }
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            if (offset + size >= buffer.Length || Client == null)
                return;
            var msg = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
            var disallowedRequests = "GET;HEAD;POST;PUT;DELETE;TRACE;OPTIONS;CONNECT;PATCH".Split(';');
            if (disallowedRequests.Any(r => msg.StartsWith(r)))
                return;
            var packets = msg.Split("%", StringSplitOptions.RemoveEmptyEntries);
            foreach (var packet in packets)
            {
                if (Client.HardwareId == null
                    && !packet.StartsWith("HI#")
                    && !packet.StartsWith("WSIP#"))
                    return;
                if (DateTime.Now.CompareTo(_banCheckTime.AddSeconds
                    (Alibi.Server.ServerConfiguration.RateLimitResetTime)) >= 0)
                {
                    _packetCount = 0;
                    _banCheckTime = DateTime.Now;
                }

                // TODO: Make a better rate limiting system that doesn't use the banning system
                if (_packetCount >= Alibi.Server.ServerConfiguration.RateLimit)
                    Client.BanIp("You have been rate limited.", Alibi.Server.ServerConfiguration.RateLimitBanLength, null);
                _packetCount++;
                _lastSentPacket = packet;
                MessageHandler.HandleMessage(Client, AOPacket.FromMessage(packet));
            }

            Client.LastAlive = DateTime.Now;
        }
    }
}