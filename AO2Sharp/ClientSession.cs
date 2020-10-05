using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using AO2Sharp.Helpers;
using AO2Sharp.Protocol;
using NetCoreServer;

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
            Console.WriteLine("Session connected: " + Socket.RemoteEndPoint);
            Client = new Client(Server as Server, this, IPAddress.Parse (((IPEndPoint)Socket.RemoteEndPoint).Address.ToString ()));

            // fuck fantaencrypt
            SendAsync(new AOPacket("decryptor", "NOENCRYPT"));
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine("Session terminated.");

            ((Server)Server).ClientsConnected.Remove(Client);
            if(Client.Connected)
                ((Server)Server).ConnectedPlayers--;
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string msg = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
            Console.WriteLine("Message recieved from " + Socket.RemoteEndPoint + ", message: " + msg);

            MessageHandler.HandleMessage(Client, AOPacket.FromMessage(msg));
        }
    }
}
