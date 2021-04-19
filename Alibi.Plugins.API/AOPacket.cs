using System.Linq;

namespace Alibi.Plugins.API
{
    /// <summary>
    /// Helper class to create packets from the API (internal server uses it's own class)
    /// </summary>
    public class AOPacket : IAOPacket
    {
        /// <summary>
        /// The type (ID) of this packet. E.g HI, RT, CT
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Data contained in this packet, that will be joined together when sent.
        /// </summary>
        public string[] Objects { get; set; }
        
        /// <param name="type">ID of the packet to be sent</param>
        /// <param name="objects">Data contained in the packet</param>
        public AOPacket(string type, params string[] objects)
        {
            Type = type;
            Objects = objects;
        }

        /// <param name="header">Blank ID of the packet to send.</param>
        public AOPacket(string header)
        {
            Type = header;
        }

        private AOPacket()
        {
        }

        /// <summary>
        /// Deconstruct an AOPacket from a string
        /// </summary>
        /// <code>
        ///     Console.WriteLine(AOPacket.FromMessage("HI#1234#").Type); // Outputs "HI"
        ///     Console.WriteLine(AOPacket.FromMessage("HI#1234#").Objects[0]); // Ouputs "1234"
        /// </code>
        /// <param name="message">The constructed packet string to deconstruct into an AOPacket</param>
        /// <returns>An AOPacket constructed from the given message.</returns>
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

        /// <summary>
        /// Encode a constructed packet string so that special characters escape properly.
        /// </summary>
        /// <param name="str">The constructed packet to encode</param>
        /// <returns>An encoded packet ready to be sent.</returns>
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