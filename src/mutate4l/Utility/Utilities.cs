using Mutate4l.Cli;
using Mutate4l.Core;
using System.Collections.Generic;
using System.Linq;

namespace Mutate4l.Utility
{
    public static class Utilities
    {
        public static Dictionary<TokenType, List<Token>> GetValidOptions(Dictionary<TokenType, List<Token>> options, TokenType[] validOptions)
        {
            var cleanedOptions = new Dictionary<TokenType, List<Token>>();
            foreach (var key in options.Keys)
            {
                if (validOptions.Contains(key))
                {
                    cleanedOptions.Add(key, options[key]);
                }
            }
            return cleanedOptions;
        }

        public static decimal MusicalDivisionToDecimal(string value)
        {
            if (value.IndexOf('/') >= 0)
            {
                return (4m / int.Parse(value.Substring(value.IndexOf('/') + 1))) * (int.Parse(value.Substring(0, value.IndexOf('/'))));
            }
            else
            {
                return int.Parse(value) * 4m;
            }
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T _a = a;
            a = b;
            b = _a;
        }

        public static SortedList<T> ToSortedList<T>(this IEnumerable<T> list)
        {
            return new SortedList<T>(list);
        }
    }
}
