using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        /// <summary>
        /// Sends an area update to the client specified, all clients if null
        /// </summary>
        /// <param name="client"></param>
        internal void AreaUpdate(AreaUpdateType type, Client client = null)
        {
            List<string> updateData = new List<string>();
            updateData.Add(((int)type).ToString());
            foreach (var area in Server.Areas)
            {
                switch (type)
                {
                    case AreaUpdateType.PlayerCount:
                        updateData.Add(PlayerCount.ToString());
                        break;
                    case AreaUpdateType.Status:
                        updateData.Add(Status);
                        break;
                    case AreaUpdateType.CourtManager:
                        updateData.Add(CurrentCourtManager);
                        break;
                    case AreaUpdateType.Locked:
                        updateData.Add(Locked ? "LOCKED" : "FREE");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            if(client == null)
                Server.Broadcast(new AOPacket("ARUP", updateData.ToArray()));
            else
                client.Send(new AOPacket("ARUP", updateData.ToArray()));
        }

        internal void FullUpdate(Client client = null)
        {
            AreaUpdate(AreaUpdateType.PlayerCount, client);
            AreaUpdate(AreaUpdateType.Status, client);
            AreaUpdate(AreaUpdateType.CourtManager, client);
            AreaUpdate(AreaUpdateType.Locked, client);
        }
    }
}
