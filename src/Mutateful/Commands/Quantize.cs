namespace Mutateful.Commands;

public class QuantizeOptions
{
    [OptionInfo(0f, 1f)]
    public decimal/*ActualDecimal*/ Amount { get; set; } = 1.0m;

    [OptionInfo(type: OptionType.Default, 1/64f)]
    public decimal[] Divisions { get; set; } = { 4/16m };

//        public decimal Threshold { get; set; } = 0.125m; // to be added

//        public decimal Magnetic { get; set; } = 0; // to be added
    
    public Clip By { get; set; } // for quantizing based on another clip (possibly very similar to constrain -start?)
}

// # desc: Quantizes a clip by the specified amount against a regular or irregular set of divisions, or even against the timings of another clip. 
public static class Quantize
{
    public static ProcessResult<Clip[]> Apply(Command command, params Clip[] clips)
    {
        var (success, msg) = OptionParser.TryParseOptions(command, out QuantizeOptions options);
        if (!success)
        {
            return new ProcessResult<Clip[]>(msg);
        }
        return Apply(options, clips);
    }

    public static ProcessResult<Clip[]> Apply(QuantizeOptions options, params Clip[] clips)
    {
        var maxLen = clips.Max(x => x.Length);
        if (options.By != null)
        {
            if (options.By.Length < maxLen)
            {
                ClipUtilities.EnlargeClipByLooping(options.By, maxLen);
            }
            options.Divisions = options.By.Notes.Select(x => x.Start).Distinct().ToArray();
        }
        else
        {
            var currentPos = 0m;
            var quantizePositions = new List<decimal>();
            var i = 0;
            while (currentPos <= maxLen)
            {
                quantizePositions.Add(currentPos);
                currentPos += options.Divisions[i % options.Divisions.Length];
                i++;
            }
            options.Divisions = quantizePositions.ToArray();
        }
        options.Amount = Math.Clamp(options.Amount, 0, 1);
        var resultClips = new Clip[clips.Length];
        
        for (var i = 0; i < clips.Length; i++)
        {
            var clip = clips[i];
            var resultClip = new Clip(clip.Length, clip.IsLooping);

            foreach (var note in clip.Notes)
            {
                var constrainedNote = note with { };
                var newStart = ClipUtilities.FindNearestNoteStartInDecimalSet(note, options.Divisions);
                constrainedNote.Start += (newStart - constrainedNote.Start) * options.Amount;
                resultClip.Add(constrainedNote);
            }
            resultClips[i] = resultClip;
        }
        return new ProcessResult<Clip[]>(resultClips);
    }
}