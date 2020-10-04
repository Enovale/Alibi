using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AO2Sharp
{
    internal class Client
    {
        public IPAddress IpAddress { get; private set; }
        public string HardwareId { get; private set; }

        public Client(IPAddress ip, string hwid = null)
        {
            IpAddress = ip;
            HardwareId = hwid;
        }
    }
}
