using AO2Sharp.Helpers;
using AO2Sharp.Plugins.API;
using System;
using System.Net;
using System.Threading.Tasks;

namespace AO2Sharp
{
    public class Client : IClient
    {
        public ClientSession Session { get; private set; }
        public Server Server { get; private set; }

        public bool Connected { get; internal set; }
        public bool Authed { get; internal set; }
        public DateTime LastAlive { get; internal set; }
        public IPAddress IpAddress { get; internal set; }
        public string HardwareId { get; internal set; }
        // I dont think this needs to be stored
        // public string ShowName { get; internal set; }
        public Area Area { get; internal set; }
        public IArea IArea => (IArea) Area;

        public string Password { get; internal set; }
        public int? Character { get; internal set; }
        public string LastSentMessage { get; internal set; }

        // Retarded pairing shit
        public int PairingWith { get; internal set; } = -1;
        public string StoredEmote { get; internal set; }
        public int StoredOffset { get; internal set; }
        public bool StoredFlip { get; internal set; }

        public Client(Server server, ClientSession session, IPAddress ip)
        {
            Server = server;
            Session = session;
            IpAddress = ip;

            server.ClientsConnected.Add(this);
        }

        public void ChangeArea(int index)
        {
            if (!Connected)
                return;

            if (index == Array.IndexOf(Server.Areas, Area))
            {
                SendOocMessage($"Can't enter area \"{Server.AreaNames[index]}\" because you're already in it.");
                return;
            }

            if (Server.Areas[index].Locked)
            {
                SendOocMessage($"Area \"{Server.AreaNames[index]}\" is locked.");
                return;
            }

            if (Character != null)
            {
                Area.TakenCharacters[(int)Character] = false;
                Area.UpdateTakenCharacters();
            }

            Area.PlayerCount--;
            Area.AreaUpdate(AreaUpdateType.PlayerCount);
            Area = Server.Areas[index];
            Area.PlayerCount++;
            Area.AreaUpdate(AreaUpdateType.PlayerCount);

            Send(new AOPacket("HP", "1", Area.DefendantHp.ToString()));
            Send(new AOPacket("HP", "2", Area.ProsecutorHp.ToString()));
            Send(new AOPacket("FA", Server.AreaNames));
            Send(new AOPacket("BN", Area.Background, Area.BackgroundPosition.ToString()));

            if (Character != null)
            {
                if (Area.TakenCharacters[(int)Character])
                {
                    Character = null;
                    Send(new AOPacket("DONE"));
                }
                else
                {
                    Area.TakenCharacters[(int)Character] = true;
                }
            }

            Area.UpdateTakenCharacters();
            SendOocMessage($"Successfully changed to area \"{Area.Name}\"");
        }

        public void KickIfBanned()
        {
            if (Server.Database.IsHdidBanned(HardwareId) || Server.Database.IsIpBanned(IpAddress.ToString()))
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

        public void BanHdid(string reason)
        {
            Server.Database.BanHdid(HardwareId, reason);
            Send(new AOPacket("KB", reason));
        }

        public void BanIp(string reason)
        {
            Server.Database.BanIp(IpAddress.ToString(), reason);
            Send(new AOPacket("KB", reason));
        }

        public void Send(AOPacket packet)
        {
            if (packet.Objects != null)
            {
                for (var i = 0; i < packet.Objects.Length; i++)
                {
                    packet.Objects[i] = packet.Objects[i].EncodeToAOPacket();
                }
            }

            Session.SendAsync(packet);
        }

        public void Send(IAOPacket packetInterface)
        {
            Send((AOPacket)packetInterface);
        }

        public void SendOocMessage(string message)
        {
            Send(new AOPacket("CT", "Server", message, "1"));
        }
    }
}
