using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer
{
    public static class StringExtensions
    {
        public static int IndexOfAny(this string s, string[] values, int startIndex, StringComparison comp)
        {
            int result = values.Select(v => s.IndexOf(v, startIndex, comp)).Where(index => index >= 0).DefaultIfEmpty(int.MaxValue).Min();
            return result < int.MaxValue ? result : -1;
        }
    }
}
