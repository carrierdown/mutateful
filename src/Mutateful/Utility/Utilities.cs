using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mutateful.Compiler;
using Mutateful.Core;

namespace Mutateful.Utility
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
                return 4m / int.Parse(value.Substring(value.IndexOf('/') + 1)) * int.Parse(value.Substring(0, value.IndexOf('/')));
            }
            return int.Parse(value) * 4m;
        }

        public static decimal BarsBeatsSixteenthsToDecimal(string value)
        {
            var parts = value.Split('.');
            var parsedParts = new int[3];
            for (var i = 0; i < parts.Length; i++)
            {
                parsedParts[i] = int.Parse(parts[i]);
                if (i > 0 && parsedParts[i] > 3) parsedParts[i] = Math.Clamp(parsedParts[i], 0, 3);
            }
            return parsedParts[0] * 4m + parsedParts[1] + parsedParts[2] * 0.25m;
        }

        public static SortedList<T> ToSortedList<T>(this IEnumerable<T> list)
        {
            return new SortedList<T>(list);
        }

        public static string GetByteArrayContentsAsString(byte[] result)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < result.Length; i++)
            {
                var res = result[i];
                sb.Append($"{res}{(i == result.Length - 1 ? "" : ", ")}");
            }
            return sb.ToString();
        }

        public static decimal RoundUpToNearestSixteenth(decimal val)
        {
            var numSixteenths = val / 0.25m;
            var numSixteenthsFloored = (int) Math.Floor(numSixteenths);
            if (numSixteenths - numSixteenthsFloored > 0)
            {
                numSixteenthsFloored += 1;
            }
            return numSixteenthsFloored * 0.25m;
        }
    }
}
