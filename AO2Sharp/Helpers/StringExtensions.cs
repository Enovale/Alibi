namespace AO2Sharp.Helpers
{
    public static class StringExtensions
    {
        public static int ToIntOrZero(this string str)
        {
            return int.TryParse(str, out int i) ? i : 0;
        }
    }
}
