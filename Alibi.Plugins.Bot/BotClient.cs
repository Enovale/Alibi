#nullable enable
using System;
using System.Net;
using Alibi.Plugins.API;

namespace Alibi.Plugins.Bot
{
    public class BotClient : IClient
    {
        public ISession Session { get; }
        public IServer ServerRef { get; }
        public bool Connected { get; set; }
        public int Auth { get; set; }
        public DateTime LastAlive { get; }
        public IPAddress IpAddress { get; }
        public string? HardwareId { get; }
        public IArea? Area { get; set; }
        public string? Position { get; set; }
        public ClientState CurrentState { get; set; }
        public string? Password { get; }
        public int? Character { get; set; }
        public string? CharacterName { get; }
        public string? OocName { get; set; }
        public string? LastSentMessage { get; set; }
        public bool Muted { get; set; }
        public CasingFlags CasingPreferences { get; set; }
        public int PairingWith { get; set; }
        public string? StoredEmote { get; set; }
        public int StoredOffset { get; set; }
        public bool StoredFlip { get; set; }

        public BotClient(IServer serverRef)
        {
            CurrentState = ClientState.NewClient;
            ServerRef = serverRef;
            Session = new DummySession(this);
            IpAddress = IPAddress.None;
            //HardwareId = new Guid().GetHashCode().ToString();
            OocName = $"Bot{HardwareId}";

            serverRef.ClientsConnected.Add(this);
            
            Receive(new AOPacket("HI", new Guid().GetHashCode().ToString()));
            Receive(new AOPacket("ID", "Alibi", "Bot"));
            Receive(new AOPacket("RD"));
            
            Connected = true;
            ChangeArea(0);
        }

        public void Receive(AOPacket packet) => ServerRef.HandlePacket(this, packet);
        
        public void Receive(string packet) => ServerRef.HandlePacket(this, AOPacket.FromMessage(packet));

        //public void Speak(string message) => Area.Broadcast()
        public void SpeakOoc(string message) => Area!.BroadcastOocMessage(message, OocName);
        public void SpeakOoc(string message, string name) => Area!.BroadcastOocMessage(message, name);

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
            if (!Connected)
                return;

            if (Area != null)
            {
                if (Character != null)
                {
                    Area.TakenCharacters[(int) Character] = false;
                    Area.UpdateTakenCharacters();
                }

                Area.PlayerCount--;
                Area.AreaUpdate(AreaUpdateType.PlayerCount);
            }

            Area = ServerRef.Areas[index];
            Area.PlayerCount++;
            Area.AreaUpdate(AreaUpdateType.PlayerCount);

            if (Character != null)
            {
                if (Area.TakenCharacters[(int) Character])
                    Character = null;
                else
                    Area.TakenCharacters[(int) Character] = true;
            }

            Area.FullUpdate(this);
            Area.UpdateTakenCharacters();
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