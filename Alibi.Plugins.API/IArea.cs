using System.Collections.Generic;

namespace Alibi.Plugins.API
{
    public interface IArea
    {
        public string Name { get; }
        public string Background { get; }
        public bool CanLock { get; }
        public bool BackgroundLocked { get; }
        public bool IniSwappingAllowed { get; }
        public string Status { get; set; }
        public string Locked { get; set; }
        public int PlayerCount { get; }
        public List<IClient> CurrentCaseManagers { get; }
        public string Document { get; set; }
        public int DefendantHp { get; set; }
        public int ProsecutorHp { get; set; }
        public bool[] TakenCharacters { get; }
        public List<IEvidence> EvidenceList { get; }

        public void Broadcast(IAOPacket packet);
        public void BroadcastOocMessage(string message);
        public void AreaUpdate(AreaUpdateType type, IClient client = null);
        public void FullUpdate(IClient client = null);
        public bool IsClientCM(IClient client);
        public void UpdateTakenCharacters();
    }
}