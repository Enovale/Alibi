using System;
using System.Net;
using System.Threading.Tasks;
using Alibi.Plugins.API;

namespace Alibi.Plugins.Bot
{
    public class BotClient : IClient
    {
        public ISession Session { get; }
        public IServer ServerRef { get; }
        public bool Connected { get; set; }
        public int Auth { get; }
        public DateTime LastAlive { get; }
        public IPAddress IpAddress { get; }
        public string? HardwareId { get; }
        public IArea? Area { get; }
        public string? Position { get; set; }
        public ClientState CurrentState { get; set; }
        public string? Password { get; }
        public int? Character { get; set; }
        public string? CharacterName { get; }
        public string? OocName { get; }
        public string? LastSentMessage { get; set; }
        public bool Muted { get; set; }
        public CasingFlags CasingPreferences { get; set; }
        public int PairingWith { get; }
        public string? StoredEmote { get; }
        public int StoredOffset { get; }
        public bool StoredFlip { get; }

        public BotClient(IServer serverRef)
        {
            CurrentState = ClientState.NewClient;
            ServerRef = serverRef;
            Session = new DummySession(this);
            IpAddress = IPAddress.None;
            HardwareId = new Guid().GetHashCode().ToString();

            serverRef.ClientsConnected.Add(this);
            Connected = true;
            
            Receive(new AOPacket("HI", HardwareId));
            Receive(new AOPacket("ID", "Alibi", "Bot"));
            Receive(new AOPacket("RD"));
        }

        public void Receive(AOPacket packet) => ServerRef.HandlePacket(this, packet);
        
        public void Receive(string packet) => ServerRef.HandlePacket(this, AOPacket.FromMessage(packet));

        public void Disconnect()
        {
            if (!Connected)
                return;
            
            ServerRef.ClientsConnected.Remove(this);
            ServerRef.ConnectedPlayers--;
            if (Area != null)
            {
                Area.PlayerCount--;
                if (Character != null)
                    Area.TakenCharacters[(int) Character] = false;
                Area.UpdateTakenCharacters();
                Area.CurrentCaseManagers.Remove(this);
                Area.AreaUpdate(AreaUpdateType.PlayerCount);
                Area.AreaUpdate(AreaUpdateType.CourtManager);
            }
            Connected = false;

            //BotPlugin.Log(LogSeverity.Info, $"[{IpAddress}] Disconnected.", true);
        }
        
        public void ChangeArea(int index)
        {
            throw new NotImplementedException();
        }

        public void KickIfBanned()
        {
        }

        public void Kick(string reason) => Disconnect();

        public string GetBanReason() => string.Empty;

        public void BanHwid(string reason, TimeSpan? expireDate, IClient? banner) => Disconnect();

        public void BanIp(string reason, TimeSpan? expireDate, IClient? banner) => Disconnect();

        public void Send(AOPacket packet)
        {
        }

        public void SendOocMessage(string message, string? sender = null)
        {
        }
    }
}