using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using AO2Sharp.Helpers;

namespace AO2Sharp
{
    internal class Client
    {
        public ClientSession Session { get; private set; }
        public Server Server { get; private set; }

        public bool Connected { get; internal set; }
        public DateTime LastAlive { get; internal set; }
        public IPAddress IpAddress { get; internal  set; }
        public string HardwareId { get; internal set; }
        public string ShowName { get; internal set; }
        public Area Area { get; internal set; }
        public string Password { get; internal set; }
        public int? Character { get; internal set; }

        public Client(Server server, ClientSession session, IPAddress ip)
        {
            Server = server;
            Session = session;
            IpAddress = ip;

            server.ClientsConnected.Add(this);
        }

        public void ChangeArea(int index)
        {
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

            Send(new AOPacket("HP", new []{"1", Area.DefendantHp.ToString()}));
            Send(new AOPacket("HP", new []{"2", Area.ProsecutorHp.ToString()}));
            Send(new AOPacket("FA", Server.AreaNames));
            Send(new AOPacket("BN", Area.Background));

            if (Area.TakenCharacters[(int)Character])
            {
                Character = null;
                Send(new AOPacket("DONE"));
            }
            else
            {
                Area.TakenCharacters[(int)Character] = true;
            }

            Area.UpdateTakenCharacters();
            SendOocMessage($"Successfully changed to area \"{Area.Name}\"");
        }

        public void Send(AOPacket packet)
        {
            Session.SendAsync(packet);

            Console.WriteLine("Send message to " + IpAddress + ": " + packet);
        }

        public void SendOocMessage(string message)
        {
            Send(new AOPacket("CT", new []{"Server", message, "1"}));
        }
    }
}
