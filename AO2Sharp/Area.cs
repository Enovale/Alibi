using AO2Sharp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using AO2Sharp.Plugins.API;

namespace AO2Sharp
{
    public class Area : IArea
    {
        [NonSerialized]
        public static readonly Area Default = new Area()
        {
            Name = "Test Area",
            Background = "gs4",
            BackgroundPosition = 0,
            BackgroundLocked = false,
            Locked = false,
            IniSwappingAllowed = true
        };

        public string Name { get; set; }
        public string Background { get; set; }
        public int BackgroundPosition { get; set; }
        public bool Locked { get; set; }
        public bool BackgroundLocked { get; set; }
        public bool IniSwappingAllowed { get; set; }
        public string Status { get; set; } = "FREE";
        [NonSerialized]
        public int PlayerCount;
        [NonSerialized]
        public string CurrentCourtManager = "FREE";
        [NonSerialized]
        public string Document;
        [NonSerialized]
        public int DefendantHp = 10;
        [NonSerialized]
        public int ProsecutorHp = 10;
        [NonSerialized]
        public bool[] TakenCharacters;
        [NonSerialized]
        public List<Evidence> EvidenceList = new List<Evidence>();

        [NonSerialized]
        internal Server Server;

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

        internal void BroadcastOocMessage(string message)
        {
            Broadcast(new AOPacket("CT", "Server", message, "1"));
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
                        updateData.Add(area.PlayerCount.ToString());
                        break;
                    case AreaUpdateType.Status:
                        updateData.Add(area.Status);
                        break;
                    case AreaUpdateType.CourtManager:
                        updateData.Add(area.CurrentCourtManager);
                        break;
                    case AreaUpdateType.Locked:
                        updateData.Add(area.Locked ? "LOCKED" : "FREE");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            if (client == null)
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

        internal void UpdateTakenCharacters()
        {
            List<string> takenData = new List<string>(Server.CharactersList.Length);
            for (var i = 0; i < Server.CharactersList.Length; i++)
            {
                takenData.Add(TakenCharacters[i] ? "-1" : "0");
            }

            Broadcast(new AOPacket("CharsCheck", takenData.ToArray()));
        }
    }
}
