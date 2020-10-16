using System.Linq;

namespace Alibi.Plugins.API
{
    public class Packet : IAOPacket
    {
        public string Type { get; set; }
        public string[] Objects { get; set; }

        public Packet(string type, params string[] objects)
        {
            Type = type;
            Objects = objects;
        }

        public Packet(string header)
        {
            Type = header;
        }

        private Packet()
        {
        }

        public static IAOPacket FromMessage(string message)
        {
            try
            {
                var packet = new Packet();
                string[] split = message.Split("#");
                packet.Type = split.First();

                packet.Objects = new string[split.Length - 2];
                for (var i = 1; i < split.Length - 1; i++) // -2 because header and footer %
                {
                    packet.Objects[i - 1] = DecodeFromAOPacket(split[i]);
                }

                return packet;
            }
            catch
            {
                return new Packet("NULL");
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

        public static implicit operator string(Packet pkt)
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
