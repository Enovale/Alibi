#nullable enable
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Alibi.Helpers;
using Alibi.Plugins.API;
using Alibi.Protocol;
using AOPacket = Alibi.Helpers.AOPacket;

namespace Alibi
{
    public class Client : IClient
    {
        public ISession Session { get; }
        public IServer ServerRef { get; }

        public bool Connected { get; internal set; }
        public int Auth { get; internal set; }
        public DateTime LastAlive { get; internal set; }
        public IPAddress IpAddress { get; internal set; }
        public string? HardwareId { get; internal set; }
        public IArea? Area { get; internal set; }
        public string? Position { get; set; }
        public ClientState CurrentState { get; set; }

        public string? Password { get; internal set; }
        public int? Character { get; set; }

        public string CharacterName => Character != null ? Server.CharactersList[(int) Character] : "Spectator";

        public string? OocName { get; internal set; }
        public string? LastSentMessage { get; set; }
        public bool Muted { get; set; } = false;

        // Retarded pairing shit
        public int PairingWith { get; internal set; } = -1;
        public string? StoredEmote { get; internal set; }
        public int StoredOffset { get; internal set; }
        public bool StoredFlip { get; internal set; }

        private DateTime _rateLimitCheckTime;
        private int _packetCount;

        public Client(Server serverRef, ISession session, IPAddress ip)
        {
            CurrentState = ClientState.NewClient;
            ServerRef = serverRef;
            Session = session;
            IpAddress = ip;

            serverRef.ClientsConnected.Add(this);
        }

        internal void OnSessionConnected()
        {
            if (((Server) ServerRef).ConnectedPlayers >= Server.ServerConfiguration.MaxPlayers)
            {
                Send(new AOPacket("BD", "Not a real ban: Max players has been reached."));
                Task.Delay(500);
                Session.Disconnect();
                return;
            }

            _rateLimitCheckTime = DateTime.Now;
            
            if (!IPAddress.IsLoopback(IpAddress) &&
                ((Server) ServerRef).ClientsConnected.Count(c => IpAddress.ToString() == c.IpAddress.ToString())
                > Server.ServerConfiguration.MaxMultiClients)
            {
                Send(new AOPacket("BD", "Not a real ban: Can't have more than " +
                                        $"{Server.ServerConfiguration.MaxMultiClients} clients at a time."));
                Task.Delay(500);
                Session.Disconnect();
                return;
            }

            KickIfBanned();

            ((Server) ServerRef).OnPlayerJoined(this);

            // fuck fantaencrypt
            Send(new AOPacket("decryptor", "NOENCRYPT"));
        }

        internal void OnSessionDisconnected()
        {
            ((Server) ServerRef).ClientsConnected.Remove(this);
            if (Connected)
            {
                ((Server) ServerRef).ConnectedPlayers--;
                ((Area) Area!).PlayerCount--;
                Connected = false;
                if (Character != null)
                    Area.TakenCharacters[(int) Character] = false;
                Area.UpdateTakenCharacters();
                Area.CurrentCaseManagers.Remove(this);
                Area.AreaUpdate(AreaUpdateType.PlayerCount);
                Area.AreaUpdate(AreaUpdateType.CourtManager);
            }
        }

        internal void OnSessionReceived(byte[] buffer, long offset, long size)
        {
            var msg = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
            var disallowedRequests = "GET;HEAD;POST;PUT;DELETE;TRACE;OPTIONS;CONNECT;PATCH".Split(';');
            if (disallowedRequests.Any(r => msg.StartsWith(r)))
                return;
            var packets = msg.Split("%", StringSplitOptions.RemoveEmptyEntries);
            foreach (var packet in packets)
            {
                if (HardwareId == null
                    && !packet.StartsWith("HI#")
                    && !packet.StartsWith("WSIP#"))
                    return;
                if (DateTime.Now.CompareTo(_rateLimitCheckTime.AddSeconds
                    (Server.ServerConfiguration.RateLimitResetTime)) >= 0)
                {
                    _packetCount = 0;
                    _rateLimitCheckTime = DateTime.Now;
                }

                // TODO: Make a better rate limiting system that doesn't use the banning system
                if (_packetCount >= Server.ServerConfiguration.RateLimit)
                    BanIp("You have been rate limited.", Server.ServerConfiguration.RateLimitBanLength,
                        null);
                _packetCount++;
                MessageHandler.HandleMessage(this, AOPacket.FromMessage(packet));
            }

            LastAlive = DateTime.Now;
        }

        public void ChangeArea(int index)
        {
            if (!Connected)
                return;

            if (Area != null)
            {
                if (index == Array.IndexOf(ServerRef.Areas, Area))
                {
                    SendOocMessage($"Can't enter area \"{ServerRef.AreaNames[index]}\" because you're already in it.");
                    return;
                }

                if (ServerRef.Areas[index].Locked == "LOCKED")
                {
                    SendOocMessage($"Area \"{ServerRef.AreaNames[index]}\" is locked.");
                    return;
                }

                if (ServerRef.Areas[index].Locked == "SPECTATABLE" && Character == null)
                {
                    SendOocMessage($"Area \"{ServerRef.AreaNames[index]}\" is spectater-only.");
                    return;
                }

                if (Character != null)
                {
                    Area.TakenCharacters[(int) Character] = false;
                    Area.UpdateTakenCharacters();
                }

                ((Area) Area!).PlayerCount--;
                Area.AreaUpdate(AreaUpdateType.PlayerCount);
            }

            Area = ServerRef.Areas[index];
            ((Area) Area).PlayerCount++;
            Area.AreaUpdate(AreaUpdateType.PlayerCount);
            Messages.RequestEvidence(this, new AOPacket("RE"));

            Send(new AOPacket("HP", "1", Area.DefendantHp.ToString()));
            Send(new AOPacket("HP", "2", Area.ProsecutorHp.ToString()));
            Send(new AOPacket("FA", ServerRef.AreaNames));
            Send(new AOPacket("BN", Area.Background));

            if (Character != null)
            {
                if (Area.TakenCharacters[(int) Character])
                {
                    Character = null;
                    Send(new AOPacket("DONE"));
                }
                else
                {
                    Area.TakenCharacters[(int) Character] = true;
                }
            }
            else
            {
                Send(new AOPacket("DONE"));
            }

            Area.FullUpdate(this);
            Area.UpdateTakenCharacters();
            SendOocMessage($"Successfully changed to area \"{Area.Name}\"");
        }

        public void KickIfBanned()
        {
            if (Server.Database.IsHwidBanned(HardwareId) || Server.Database.IsIpBanned(IpAddress.ToString()))
            {
                Send(new AOPacket("BD", GetBanReason()));
                Task.Delay(500).Wait();
                Session.Disconnect();
            }
        }

        public void Kick(string reason)
        {
            Send(new AOPacket("KK", reason));
            Task.Delay(500).Wait();
            Session.Disconnect();
        }

        public string GetBanReason()
        {
            return Server.Database.GetBanReason(IpAddress.ToString());
        }

        public void BanHwid(string reason, TimeSpan? expireDate, IClient? banner)
        {
            if (!((Server)ServerRef).OnBan(ServerRef.FindUser(HardwareId!)!, banner, ref reason, expireDate))
                return;
            Server.Database.BanHwid(HardwareId, reason, expireDate);
            Send(new AOPacket("KB", reason));
            Task.Delay(500).Wait();
            Session.Disconnect();
        }

        public void BanIp(string reason, TimeSpan? expireDate, IClient? banner)
        {
            foreach (var hwid in Server.Database.GetHwidsfromIp(IpAddress.ToString()))
                ServerRef.FindUser(hwid)?.BanHwid(reason, expireDate, banner);
        }

        public void Send(IAOPacket packet)
        {
            if (packet.Objects != null)
                for (var i = 0; i < packet.Objects.Length; i++)
                    packet.Objects[i] = packet.Objects[i].EncodeToAOPacket();

            Session.SendAsync((AOPacket)packet);
        }

        public void SendOocMessage(string message, string? sender = null)
        {
            Send(new AOPacket("CT", sender ?? "Server", message, "1"));
        }
    }
}