using AO2Sharp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using AO2Sharp.Plugins.API;
using Newtonsoft.Json;

namespace AO2Sharp
{
    public class Area : IArea
    {
        public string Name { get; set; } = "AreaName";
        public string Background { get; set; } = "gs4";
        public bool CanLock { get; set; } = true;
        public bool BackgroundLocked { get; set; } = false;
        public bool IniSwappingAllowed { get; set; } = true;
        public string Status { get; set; } = "FREE";
        [JsonIgnore]
        public bool Locked { get; set; } = false;
        [JsonIgnore]
        public int PlayerCount { get; set; } = 0;
        [JsonIgnore]
        public Client CurrentCourtManager { get; set; } = null;
        [JsonIgnore]
        public IClient ICurrentCourtManager
        {
            get => CurrentCourtManager;
            set => CurrentCourtManager = (Client)value;
        }

        [JsonIgnore]
        public string Document { get; set; }
        [JsonIgnore]
        public int DefendantHp { get; set; } = 10;
        [JsonIgnore]
        public int ProsecutorHp { get; set; } = 10;
        [JsonIgnore]
        public bool[] TakenCharacters { get; set; }
        [JsonIgnore]
        public List<Evidence> EvidenceList { get; set; } = new List<Evidence>();
        [JsonIgnore]
        public List<IEvidence> IEvidenceList => (List<IEvidence>)EvidenceList.Cast<IEvidence>();

        [JsonIgnore]
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
                        if(area.CurrentCourtManager == null)
                            updateData.Add("FREE");
                        else
                            updateData.Add(area.CurrentCourtManager.CharacterName!);
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
