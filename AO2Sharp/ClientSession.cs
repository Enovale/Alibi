using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AO2Sharp.Helpers;
using NetCoreServer;

namespace AO2Sharp
{
    internal class ClientSession : TcpSession
    {
        public ClientSession(TcpServer server) : base(server)
        {
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
