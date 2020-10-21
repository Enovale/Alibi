namespace Alibi.Plugins.API
{
    public interface IEvidence
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Photo { get; set; }

        public string ToPacket();
    }
}