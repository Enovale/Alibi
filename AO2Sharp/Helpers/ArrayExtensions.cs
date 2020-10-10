using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AO2Sharp.Helpers
{
    public static class ArrayExtensions
    {
        public static T[] Concatenate<T>(this T[] first, T[] second)
        {
            if (first == null)
            {
                return second;
            }
            if (second == null)
            {
                return first;
            }

            return first.Concat(second).ToArray();
        }
    }
}
