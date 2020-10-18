namespace Alibi.Plugins.Cerberus
{
    public class CerberusConfiguration
    {
        public bool StripZalgo { get; set; } = true;
        
        public int MaxIcMessagesPerSecond { get; set; } = 3;
        public int IcMuteLengthInSeconds { get; set; } = 5;
        public int MaxOocMessagesPerSecond { get; set; } = 3;
        public int OocMuteLengthInSeconds { get; set; } = 5;
        public int MaxMusicMessagesPerSecond { get; set; } = 2;
        public int MusicMuteLengthInSeconds { get; set; } = 5;
    }
}