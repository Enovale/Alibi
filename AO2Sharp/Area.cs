using AO2Sharp.Helpers;
using AO2Sharp.Plugins.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AO2Sharp
{
    public class Area : IArea
    {
        public string Name { get; set; } = "AreaName";
        public string Background { get; set; } = "gs4";
        public bool CanLock { get; set; } = true;
        public bool BackgroundLocked { get; set; } = false;
        public bool IniSwappingAllowed { get; set; } = true;
        /// <summary>
        /// Permission level needed to modify evidence
        /// 0 = FFA
        /// 1 = CM
        /// 2 = No-one
        /// </summary>
        public int EvidenceModifications { get; set; } = 0;
        public string Status { get; set; } = "IDLE";
        [JsonIgnore]
        public string Locked { get; set; } = "FREE";
        [JsonIgnore]
        public int PlayerCount { get; set; } = 0;
        [JsonIgnore]
        public List<IClient> CurrentCaseManagers { get; } = new List<IClient>();

        [JsonIgnore]
        public string Document { get; set; }
        [JsonIgnore]
        public int DefendantHp { get; set; } = 10;
        [JsonIgnore]
        public int ProsecutorHp { get; set; } = 10;
        [JsonIgnore]
        public bool[] TakenCharacters { get; set; }
        [JsonIgnore]
        public List<IEvidence> EvidenceList { get; } = new List<IEvidence>();

        [JsonIgnore]
        internal Server Server;

        public void Broadcast(IAOPacket packet)
        {
            var clientQueue = new Queue<IClient>(Server.ClientsConnected);
            while (clientQueue.Any())
            {
                var client = clientQueue.Dequeue();
                if (client.Connected && client.Area == this)
                    client.Send(packet);
            }
        }

        public void BroadcastOocMessage(string message)
        {
            Broadcast(new AOPacket("CT", "ServerRef", message, "1"));
        }

        /// <summary>
        /// Sends an area update to the client specified, all clients if null
        /// </summary>
        /// <param name="client"></param>
        public void AreaUpdate(AreaUpdateType type, IClient client = null)
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
                        if (area.CurrentCaseManagers.Count <= 0)
                            updateData.Add("FREE");
                        else
                            updateData.Add(
                                string.Join(',', area.CurrentCaseManagers.Select(c => c.CharacterName)));
                        break;
                    case AreaUpdateType.Locked:
                        updateData.Add(area.Locked);
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

        public void FullUpdate(IClient client = null)
        {
            AreaUpdate(AreaUpdateType.PlayerCount, client);
            AreaUpdate(AreaUpdateType.Status, client);
            AreaUpdate(AreaUpdateType.CourtManager, client);
            AreaUpdate(AreaUpdateType.Locked, client);
        }

        public bool IsClientCM(IClient client) => CurrentCaseManagers.Contains(client);

        public void UpdateTakenCharacters()
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
