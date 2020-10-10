namespace AO2Sharp.Plugins.API
{
    public interface IAOPacket
    {
        public string Type { get; set; }
        public string[] Objects { get; set; }
    }
}
