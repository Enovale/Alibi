using AO2Sharp.Plugins.API;
using System;
using System.Linq;

namespace AO2Sharp.Helpers
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
                string[] split = message.Split("#");
                packet.Type = split.First();

                packet.Objects = new string[split.Length - 2];
                for (var i = 1; i < split.Length - 1; i++) // -2 because header and footer %
                {
                    packet.Objects[i - 1] = split[i].DecodeFromAOPacket();
                }

                return packet;
            }
            catch (Exception e)
            {
                if (Server.ServerConfiguration.VerboseLogs)
                    Server.Logger.Log(LogSeverity.Error, " " + e.Message + "\n" + e.StackTrace, true);
                else
                    Server.Logger.Log(LogSeverity.Error, " " + e.Message);
                return new AOPacket("NULL");
            }
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
