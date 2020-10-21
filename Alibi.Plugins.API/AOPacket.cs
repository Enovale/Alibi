using System.Linq;

namespace Alibi.Plugins.API
{
    public class AOPacket : IAOPacket
    {
        public string Type { get; set; }
        public string[] Objects { get; set; }
        
        public AOPacket(string type, params string[] objects)
        {
            Type = type;
            Objects = objects;
        }

        public AOPacket(string header)
        {
            Type = header;
        }

        private AOPacket()
        {
        }

        public static IAOPacket FromMessage(string message)
        {
            try
            {
                var packet = new AOPacket();
                var split = message.Split("#");
                packet.Type = split.First();

                packet.Objects = new string[split.Length - 2];
                for (var i = 1; i < split.Length - 1; i++) // -2 because header and footer %
                    packet.Objects[i - 1] = DecodeFromAOPacket(split[i]);

                return packet;
            }
            catch
            {
                return new AOPacket("NULL");
            }
        }

        public static string EncodeToAOPacket(string str)
        {
            return str.Replace("%", "<percent>").Replace("#", "<num>").Replace("$", "<dollar>").Replace("&", "<and>");
        }

        public static string DecodeFromAOPacket(string str)
        {
            return str.Replace("<percent>", "%").Replace("<num>", "#").Replace("<dollar>", "$").Replace("<and>", "&");
        }

        public static implicit operator string(AOPacket pkt)
        {
            if (pkt.Objects == null) return pkt.Type + "#%";

            var final = pkt.Type + "#";
            foreach (var o in pkt.Objects) final += o + "#";

            final += "%";
            return final;
        }
    }
}