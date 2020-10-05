using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AO2Sharp.Helpers;

namespace AO2Sharp
{
    public class Area
    {
        [NonSerialized] public static readonly Area Default = new Area()
        {
            Name = "Test Area",
            Background = "gs4",
            BackgroundLocked = false,
            Locked = false
        };

        public string Name;
        public string Background;
        public bool Locked;
        public bool BackgroundLocked;
        [NonSerialized]
        public int PlayerCount;
        [NonSerialized]
        public string CurrentCourtManager = "FREE";
        [NonSerialized]
        public string Status = "FREE";
        [NonSerialized]
        public int DefendantHp = 10;
        [NonSerialized]
        public int ProsecutorHp = 10;

        [NonSerialized] public bool[] TakenCharacters;

        [NonSerialized] internal Server Server;

        internal void Broadcast(AOPacket packet)
        {
            var clientQueue = new Queue<Client>(Server.ClientsConnected);
            while (clientQueue.Any())
            {
                var client = clientQueue.Dequeue();
                if (client.Connected && client.Area == this)
                    client.Send(packet);
            }
        }
    }
}
