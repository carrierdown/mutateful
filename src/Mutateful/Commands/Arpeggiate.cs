namespace Mutateful.Commands;

public class ArpeggiateOptions
{
    [OptionInfo(min: 1, max: 32)]
    public int Rescale { get; set; } = 2; // todo: make it a percentage of the arpclip instead? With possibility of scaling the number of events further based on the duration of the current event compared to the longest event in the clip.

    public bool RemoveOffset { get; set; } = true;

    public Clip By { get; set; }

    // Could add shaping here as well - easein/out etc..
}

// # desc: Arpeggiates the given clip using another clip, or itself.
public static class Arpeggiate
{
    public static ProcessResult<Clip[]> Apply(Command command, params Clip[] clips)
    {
        var (success, msg) = OptionParser.TryParseOptions(command, out ArpeggiateOptions options);
        if (!success)
        {
            return new ProcessResult<Clip[]>(msg);
        }
        return Apply(options, clips);
    }

    // Add option to dynamically set # of events that should be rescaled to another note, probably via velocity.
    public static ProcessResult<Clip[]> Apply(ArpeggiateOptions options, params Clip[] clips)
    {
        Clip arpSequence = options.By ?? clips[0];

        foreach (var clip in clips)
        {
            // ClipUtilities.Monophonize(clip);
        }
        var processedClips = new List<Clip>(clips.Length);

        // If arp sequence doesn't start at zero and remove offset is specified, make it start at zero
        if (arpSequence.Notes[0].Start != 0 && options.RemoveOffset)
        {
            foreach (var arpNote in arpSequence.Notes)
            {
                arpNote.Start -= arpSequence.Notes[0].Start;
            }
        }

        var count = Math.Min(arpSequence.Notes.Count, options.Rescale);
        var arpNotes = arpSequence.Notes.Take(count);
        var actualLength = arpNotes.Last().Start + arpNotes.Last().Duration;
        // Rescale arp events to the range 0-1
        foreach (var arpNote in arpNotes)
        {
            arpNote.Start = arpNote.Start / actualLength;
            arpNote.Duration = arpNote.Duration / actualLength;
        }
        
        foreach (var clip in clips)
        {
            var resultClip = new Clip(clip.Length, clip.IsLooping);
            for (var i = 0; i < clip.Notes.Count; i++)
            {
                var note = clip.Notes[i];
                var processedNotes = new List<NoteEvent>(count);

                var ix = 0;
                foreach (var currentArpNote in arpNotes)
                {
                    var processedNote = currentArpNote with {};
                    processedNote.Start = note.Start + (processedNote.Start * note.Duration);
                    processedNote.Duration *= note.Duration;
                    processedNote.Pitch = note.Pitch + arpSequence.RelativePitch(ix);
                    processedNotes.Add(processedNote);
                    ix++;
                }
                resultClip.Notes.AddRange(processedNotes);
            }
            processedClips.Add(resultClip);
        }
        return new ProcessResult<Clip[]>(processedClips.ToArray());
    }
}
