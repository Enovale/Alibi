#nullable enable
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Alibi.Helpers;
using Alibi.Plugins.API;
using AOPacket = Alibi.Helpers.AOPacket;

namespace Alibi
{
    public class Client : IClient
    {
        public ClientSession Session { get; }
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

        public string? CharacterName => Character != null ? Server.CharactersList[(int) Character] : null;

        public string? OocName { get; internal set; }
        public string? LastSentMessage { get; set; }
        public bool Muted { get; set; } = false;

        // Retarded pairing shit
        public int PairingWith { get; internal set; } = -1;
        public string? StoredEmote { get; internal set; }
        public int StoredOffset { get; internal set; }
        public bool StoredFlip { get; internal set; }

        public Client(Server serverRef, ClientSession session, IPAddress ip)
        {
            CurrentState = ClientState.NewClient;
            ServerRef = serverRef;
            Session = session;
            IpAddress = ip;

            serverRef.ClientsConnected.Add(this);
        }

        public void ChangeArea(int index)
        {
            if (!Connected)
                return;

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

            // Mostly to make the nullable warnings to shut up
            if (Area == null)
                Area = ServerRef.Areas.First();

            if (Character != null)
            {
                Area.TakenCharacters[(int) Character] = false;
                Area.UpdateTakenCharacters();
            }

            ((Area) Area).PlayerCount--;
            Area.AreaUpdate(AreaUpdateType.PlayerCount);
            Area = ServerRef.Areas[index];
            ((Area) Area).PlayerCount++;
            Area.AreaUpdate(AreaUpdateType.PlayerCount);

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

        public void BanHwid(string reason, TimeSpan? expireDate)
        {
            if (!ServerRef.OnBan(ServerRef.FindUser(HardwareId!)!, ref reason, expireDate))
                return;
            Server.Database.BanHwid(HardwareId, reason, expireDate);
            Send(new AOPacket("KB", reason));
            Task.Delay(500).Wait();
            Session.Disconnect();
        }

        public void BanIp(string reason, TimeSpan? expireDate)
        {
            foreach (var hwid in Server.Database.GetHwidsfromIp(IpAddress.ToString()))
                ServerRef.FindUser(hwid)?.BanHwid(reason, expireDate);
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