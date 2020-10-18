namespace Alibi.Plugins.Cerberus
{
    public class CerberusConfiguration
    {
        public int MaxIcMessagesPerSecond { get; set; } = 3;
        public int IcMuteLengthInSeconds { get; set; } = 5;
    }
}