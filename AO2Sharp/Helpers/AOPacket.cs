using System;
using System.Collections.Generic;
using System.Text;

namespace AO2Sharp.Helpers
{
    internal static class AOPacket
    {
        public static string CreatePacket(string name, object obj)
        {
            return CreatePacket(name, new[] {obj});
        }

        public static string CreatePacket(string name, object[] data)
        {
            string final = name + "#";
            foreach (var o in data)
            {
                final += o + "#";
            }

            final += "%";
            return final;
        }
    }
}
