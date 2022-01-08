namespace Mutateful.Core;

public record struct ClipReference(int Track, int Clip)
{
    public static ClipReference FromString(string clipRef)
    {
        if (TryParse(clipRef, out var result))
        {
            return result;
        }
        throw new ArgumentException($"Unable to parse given ClipReference: {clipRef}");
    }

    private static bool TryParse(string clipRef, out ClipReference clipReference)
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
        var digits = new List<char>();

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

    private static char ToSpreadshimalDigit(int val)
    {
        return (char)(65 + val);
    }

    private static int FromSpreadshimalDigit(char val)
    {
        return val switch
        {
            >= 'A' and <= 'Z' => val - 64,
            >= 'a' and <= 'z' => val - 96,
            _ => 0
        };
    }

    public override string ToString()
    {
        return $"{ToSpreadshimal(Track)}{Clip}";
    }

    public static readonly ClipReference Empty = new ClipReference(0, 0);
}