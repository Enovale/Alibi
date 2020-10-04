using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using AO2Sharp.Helpers;
using NetCoreServer;

namespace AO2Sharp
{
    internal class ClientSession : TcpSession
    {
        public Client Client;

        public ClientSession(TcpServer server) : base(server)
        {
            Client = new Client(IPAddress.Parse (((IPEndPoint)Socket.RemoteEndPoint).Address.ToString ()));
        }

        protected override void OnConnected()
        {
            Console.WriteLine("Session connected: " + this.Socket.RemoteEndPoint);

            // fuck fantaencrypt
            SendAsync(AOPacket.CreatePacket("decryptor", "NOENCRYPT"));
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine("Session terminated.");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string msg = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
            Console.WriteLine("Message recieved from " + Socket.RemoteEndPoint + ", message: " + msg);
        }
    }
}
