using System;
using System.Linq;
using System.Text;

namespace Alibi.Plugins.API
{
    /// <summary>
    /// Helper class to create packets from the API
    /// </summary>
    /// <remarks>
    /// (internal server uses it's own class)
    /// </remarks>
    public class AOPacket
    {
        /// <summary>
        /// The type (ID) of this packet.
        /// </summary>
        /// <example>
        /// HI, RT, CT
        /// </example>
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
            Objects = new string[0];
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
                return new AOPacket("NULL", e.Message);
            }
        }

        /// <summary>
        /// Encode a constructed packet string so that special characters escape properly.
        /// </summary>
        /// <param name="str">The constructed packet to encode</param>
        /// <returns>An encoded packet ready to be sent.</returns>
        /// <code>
        /// EncodeToAOPacket("HI#1234#%"); // returns "HI<num>1234<num><percent>"
        /// </code>
        public static string EncodeToAOPacket(string str)
        {
            return str.Replace("%", "<percent>").Replace("#", "<num>").Replace("$", "<dollar>").Replace("&", "<and>");
        }

        /// <summary>
        /// Decode a constructed packet string so that special characters are no longer escaped.
        /// </summary>
        /// <param name="str">The constructed packet to decode</param>
        /// <returns>A decoded packet that should NOT be sent.</returns>
        /// <code>
        /// EncodeToAOPacket("HI<num>1234<num><percent>"); // returns "HI#1234#%"
        /// </code>
        public static string DecodeFromAOPacket(string str)
        {
            return str.Replace("<percent>", "%").Replace("<num>", "#").Replace("<dollar>", "$").Replace("<and>", "&");
        }

        /// <summary>
        /// Converts this packet into a decoded, constructed string implicitly.
        /// </summary>
        /// <param name="pkt">The packet object</param>
        /// <returns>A decoded, constructed string</returns>
        /// <code>
        /// Console.WriteLine(new AOPacket("HI", new[] { 1234 })) // Prints "HI#1234#%"
        /// </code>
        public static implicit operator string(AOPacket pkt)
        {
            if (pkt.Objects == null) return pkt.Type + "#%";

            var final = new StringBuilder(pkt.Type + "#");
            foreach (var o in pkt.Objects) final.Append(o + "#");

            final.Append('%');
            return final.ToString();
        }
    }
}