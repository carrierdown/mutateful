namespace Mutateful.Commands;

// # desc: Makes the clip monophonic by removing any overlapping notes. Lower notes have precedence over higher notes.
public static class Monophonize
{
    // TODO: Add option to cut overlapping events, so that more of the original clip is preserved
    
    public static ProcessResult<Clip[]> Apply(params Clip[] clips)
    {
        var resultClips = ClipUtilities.CreateEmptyPlaceholderClips(clips);

        for (var i = 0; i < clips.Length; i++)
        {
            var clip = clips[i];
            var resultClip = resultClips[i];
            foreach (var note in clip.Notes) AddNoteCutting(resultClip, note with {});
        }

        return Filter.Apply(new FilterOptions(), resultClips);
    }

    private static void AddNoteCutting(Clip clip, NoteEvent noteToAdd)
    {
        var collidingNotes = clip.Notes.Where(x => noteToAdd.StartsInsideIntervalInclusive(x.Start, x.End)).ToArray();
        if (collidingNotes.Length > 0)
        {
            foreach (var note in collidingNotes)
            {
                if (note.Start == noteToAdd.Start && noteToAdd.Duration > note.Duration) // largest note wins in the case of a collision
                {
                    clip.Notes.RemoveAt(clip.Notes.IndexOf(note));
                }
                else
                {
                    // todo: maybe add extra logic to add back previous note if it spans the length of the note being added currently
                    note.Duration = noteToAdd.Start - note.Start;
                }
            }
        }
        clip.Notes.Add(noteToAdd);
    }

}