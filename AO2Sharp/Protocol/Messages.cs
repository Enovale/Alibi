using System;
using System.Collections.Generic;
using System.Text;

namespace AO2Sharp.Protocol
{
    internal static class Messages
    {
        [MessageHandler("HI")]
        internal static void HardwareId(Server server, string message)
        {
            Console.WriteLine("Recieved hardware ID: " + message.Split("#")[1]);
        }
    }
}
