namespace AO2Sharp
{
    public class Evidence
    {
        public string Name;
        public string Description;
        public string Photo;

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
