using System.Globalization;
using System.Text;

namespace Alibi.Plugins.Cerberus
{
    public static class StringExtensions
    {
        public static string LimitDiacritics(this string value, int limit)
        {
            var strFormD = value.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            var i = 0;
            foreach (var c in strFormD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if(uc != UnicodeCategory.NonSpacingMark) {
                    sb.Append(c);
                    i = 0;
                } else {
                    if (i < limit) {
                        sb.Append(c);
                    }
                    i++;
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}