namespace Alibi.Plugins.API
{
    public static class StringExtensions
    {
        public static int ToIntOrZero(this string str)
        {
            return int.TryParse(str, out var i) ? i : 0;
        }

        public static string EncodeToAOPacket(this string str)
        {
            return str.Replace("%", "<percent>").Replace("#", "<num>").Replace("$", "<dollar>").Replace("&", "<and>");
        }

        public static string DecodeFromAOPacket(this string str)
        {
            return str.Replace("<percent>", "%").Replace("<num>", "#").Replace("<dollar>", "$").Replace("<and>", "&");
        }
    }
}