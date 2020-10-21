using System;
using System.Linq;
using Alibi.Plugins.API;

namespace Alibi.Helpers
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

        public static AOPacket FromMessage(string message)
        {
            try
            {
                var packet = new AOPacket();
                var split = message.Split("#");
                packet.Type = split.First();

                if (split.Length <= 1)
                {
                    packet.Objects = Array.Empty<string>();
                    return packet;
                }

                packet.Objects = new string[split.Length - 2];
                for (var i = 1; i < split.Length - 1; i++) // -2 because header and footer %
                    packet.Objects[i - 1] = split[i].DecodeFromAOPacket();

                return packet;
            }
            catch (Exception e)
            {
                Server.Logger.Log(LogSeverity.Error, " " + e);
                return new AOPacket("NULL");
            }
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