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

        public void Send(AOPacket packet)
        {
            Session.SendAsync(packet);

            Console.WriteLine("Send message to " + IpAddress + ": " + packet);
        }
    }
}
