using System.Collections.Generic;

namespace AO2Sharp.Plugins.API
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
        public List<IClient> ICurrentCourtManagers { get; set; }
        public string Document { get; set; }
        public int DefendantHp { get; set; }
        public int ProsecutorHp { get; set; }
        public bool[] TakenCharacters { get; }
        public List<IEvidence> IEvidenceList { get; }
    }
}
