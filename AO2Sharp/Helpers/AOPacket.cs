using System.Linq;

namespace AO2Sharp.Helpers
{
    internal class AOPacket
    {
        public string Type;
        public string[] Objects;

        public AOPacket(string type, string[] objects)
        {
            Type = type;
            Objects = objects;
        }

        public AOPacket(string type, string obj)
        {
            Type = type;
            Objects = new[] {obj};
        }

        public AOPacket(string header)
        {
            Type = header;
        }

        private AOPacket()
        {
        }

        public static AOPacket FromMessage(string message)
        {
            var packet = new AOPacket();
            string[] split = message.Split("#");
            packet.Type = split.First();

            packet.Objects = new string[split.Length - 2];
            for (var i = 1; i < split.Length - 1; i++) // -2 because header and footer %
            {
                packet.Objects[i - 1] = split[i];
            }

            return packet;
        }

        public static implicit operator string(AOPacket pkt)
        {
            if (pkt.Objects == null)
            {
                return pkt.Type + "#%";
            }

            string final = pkt.Type + "#";
            foreach (var o in pkt.Objects)
            {
                final += o + "#";
            }

            final += "%";
            return final;
        }
    }
}
