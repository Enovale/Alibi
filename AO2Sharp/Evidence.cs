using AO2Sharp.Plugins.API;

namespace AO2Sharp
{
    public class Evidence : IEvidence
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Photo { get; set; }

        public Evidence(string name, string description, string photo)
        {
            Name = name;
            Description = description;
            Photo = photo;
        }

        public string ToPacket()
        {
            return $"{Name}&{Description}&{Photo}";
        }
    }
}
