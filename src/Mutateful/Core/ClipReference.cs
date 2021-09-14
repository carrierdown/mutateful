using System;
using System.Collections.Generic;

namespace Mutateful.Core
{
    public struct ClipReference
    {
        public int Track { get; set; }
        public int Clip { get; set; }

        public ClipReference(int track, int clip)
        {
            Track = track;
            Clip = clip;
        }

        public static ClipReference FromString(string clipRef)
        {
            if (TryParse(clipRef, out var result))
            {
                return result;
            }
            throw new ArgumentException($"Unable to parse given ClipReference: {clipRef}");
        }

        public static bool TryParse(string clipRef, out ClipReference clipReference)
        {
            clipReference = new ClipReference(0, 0);
            var val = clipRef.ToUpperInvariant();
            int lastAlphaIx = 0;

            while (lastAlphaIx < val.Length && val[lastAlphaIx] >= 'A' && val[lastAlphaIx] <= 'Z') lastAlphaIx++;

            if (lastAlphaIx == 0) return false;
            clipReference.Track = FromSpreadshimal(val[..lastAlphaIx]);
            var clipParsedSuccessfully = int.TryParse(val[lastAlphaIx..], out var clip);
            clipReference.Clip = clip;
            return clipParsedSuccessfully;
        }

        public static string ToSpreadshimal(int val)
        {
            if (val <= 0) return "";
            List<char> digits = new List<char>();

            while (val-- > 0)
            {
                var remainder = val % 26;
                val /= 26;
                digits.Add(ToSpreadshimalDigit(remainder));
            }
            digits.Reverse();
            return new string(digits.ToArray());
        }

        public static int FromSpreadshimal(string val)
        {
            if (val.Length == 0) return 0;
            var digits = val.ToCharArray();
            var decimalVal = 0;

            for (var i = 1; i <= digits.Length; i++)
            {
                decimalVal += FromSpreadshimalDigit(digits[^i]) * (int) Math.Pow(26, i - 1);
            }
            return decimalVal;
        }
        
        public static char ToSpreadshimalDigit(int val)
        {
            return (char)(65 + val);
        }

        public static int FromSpreadshimalDigit(char val)
        {
            if (val >= 'A' && val <= 'Z') return val - 64;
            if (val >= 'a' && val <= 'z') return val - 96;
            return 0;
        }

        public override string ToString()
        {
            return $"{ToSpreadshimal(Track)}{Clip}";
        }

        public static readonly ClipReference Empty = new ClipReference(0, 0);
    }
}