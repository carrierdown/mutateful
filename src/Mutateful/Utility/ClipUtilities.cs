namespace Mutateful.Utility;

public static class ClipUtilities
{
    public static IEnumerable<NoteEvent> RescaleNotes(List<NoteEvent> notes, decimal factor)
    {
        foreach (var note in notes)
        {
            note.Start *= factor;
            note.Duration *= factor;
        }
        return notes;
    }

    public static Clip SplitNotesAtEvery(Clip clip, decimal[] timespans)
    {
        decimal currentPosition = 0;
        var timespanIx = 0;
        while (currentPosition < clip.Length)
        {
            int i = 0;
            while (i < clip.Notes.Count)
            {
                NoteEvent note = clip.Notes[i];
                if (note.Start > currentPosition) break;
                if (note.Start < currentPosition && (note.Start + note.Duration) > currentPosition)
                {
                    // note runs across range boundary - split it
                    decimal rightSplitDuration = note.Start + note.Duration - currentPosition;
                    note.Duration = currentPosition - note.Start;
                    clip.Notes.Add(new NoteEvent(note.Pitch, currentPosition, rightSplitDuration, note.Velocity));
                    // For regular list: (though possible bug if split note spans more than one range unit)
                    //Note newNote = new Note(note.Pitch, currentPosition, rightSplitDuration, note.Velocity);
                    //notes.Insert(i + 1, newNote);
                    //i++;
                }
                i++;
            }
            currentPosition = currentPosition + timespans[timespanIx++ % timespans.Length];
        }
        return clip;
    }
    
    public static List<NoteEvent> GetNotesInRangeAtPosition(decimal start, decimal end, SortedList<NoteEvent> notes, decimal position)
    {
        var results = new List<NoteEvent>();
        var notesFromRange = GetNotesInRange(start, end, notes);

        foreach (var note in notesFromRange)
        {
            note.Start = note.Start - start + position;
        }
        results.AddRange(notesFromRange);
        return results;
    }

    public static List<NoteEvent> GetSplitNotesInRangeAtPosition(decimal start, decimal end, SortedList<NoteEvent> notes, decimal position)
    {
        List<NoteEvent> results = new List<NoteEvent>();
        foreach (var note in notes)
        {
            if (note.End < start) continue;
            if (note.Start > end) break;
            if (note.InsideIntervalInclusive(start, end))
            {
                AddNote(new NoteEvent(note.Pitch, note.Start - start + position, note.Duration, note.Velocity), results);
            }
            else if (note.CoversInterval(start, end))
            {
                AddNote(new NoteEvent(note.Pitch, position, end - start, note.Velocity), results);
            }
            else if (note.CrossesStartOfIntervalInclusive(start, end))
            {
                AddNote(new NoteEvent(note.Pitch, position, (note.Start + note.Duration) - start, note.Velocity), results);
            }
            else if (note.CrossesEndOfIntervalInclusive(start, end)) {
                AddNote(new NoteEvent(note.Pitch, note.Start - start + position, end - note.Start, note.Velocity), results);
            }
        }
        return results;
    }
    public static void AddNote(NoteEvent note, List<NoteEvent> notes)
    {
        if (note.Duration > 0.0049m)
        {
            notes.Add(note);
        }
    }

    public static List<NoteEvent> GetNotesInRange(decimal start, decimal end, SortedList<NoteEvent> notes)
    {
        var results = new List<NoteEvent>();

        foreach (var note in notes)
        {
            if (note.Start > end)
            {
                break;
            }
            if (note.Start >= start && note.Start < end)
            {
                results.Add(new NoteEvent(note.Pitch, note.Start, note.Duration, note.Velocity));
            }
        }
        return results;
    }

    public static decimal FindNearestNoteStartInSet(NoteEvent needle, SortedList<NoteEvent> haystack)
    {
        var nearestIndex = 0;
        decimal? nearestDelta = null;

        for (int i = 0; i < haystack.Count; i++)
        {
            if (nearestDelta == null)
            {
                nearestDelta = Math.Abs(needle.Start - haystack[i].Start);
            }
            decimal currentDelta = Math.Abs(needle.Start - haystack[i].Start);
            if (currentDelta < nearestDelta)
            {
                nearestDelta = currentDelta;
                nearestIndex = i;
            }
        }
        return haystack[nearestIndex].Start;
    }        
    
    public static decimal FindNearestNoteStartInDecimalSet(NoteEvent needle, decimal[] haystack)
    {
        var nearestIndex = 0;
        decimal? nearestDelta = null;

        for (int i = 0; i < haystack.Length; i++)
        {
            if (nearestDelta == null)
            {
                nearestDelta = Math.Abs(needle.Start - haystack[i]);
            }
            decimal currentDelta = Math.Abs(needle.Start - haystack[i]);
            if (currentDelta < nearestDelta)
            {
                nearestDelta = currentDelta;
                nearestIndex = i;
            }
        }
        return haystack[nearestIndex];
    }

    // Simple algorithm for finding nearest note in a list of note events
    public static int FindNearestNotePitchInSet(NoteEvent needle, SortedList<NoteEvent> haystack)
    {
        int nearestIndex = 0;
        int? nearestDelta = null;

        for (int i = 0; i < haystack.Count; i++)
        {
            int needlePitch = needle.Pitch % 12;
            int haystackPitch = haystack[i].Pitch % 12;
            int currentDelta = Math.Min(Math.Abs(needlePitch - haystackPitch), Math.Abs(needlePitch - 12 - haystackPitch));
            if (nearestDelta == null || currentDelta < nearestDelta)
            {
                nearestDelta = currentDelta;
                nearestIndex = i;
            }
        }
        return haystack[nearestIndex].Pitch;
    }

    // More "musical" algorithm for finding nearest note which takes into account notes in other octaves that might be closer in note value than notes in the same octave.
    public static int FindNearestNotePitchInSetMusical(NoteEvent needle, SortedList<NoteEvent> haystack, bool normalizeOctave = true)
    {
        int nearestPitch = FindNearestNotePitchInSet(needle.Pitch, haystack.Select(x => x.Pitch).ToArray());
        if (normalizeOctave)
        {
            return ((nearestPitch - needle.Pitch) % 12) + needle.Pitch;
        }
        return nearestPitch;
    }

    public static int FindNearestNotePitchInSet(int needle, int[] haystack)
    {
        (int nearestDelta, int[] nearestIxs, int[] octaves) = FindNearestDelta(needle, haystack);

        // check exact match
        for (int i = 0; i < haystack.Length; i++)
        {
            int currentDelta = Math.Abs(needle - haystack[i]);
            if (currentDelta == nearestDelta)
            {
                return haystack[i];
            }
        }
        // get closest matching octave
        int curOctave = needle / 12;
        int nearestIx = 0;
        int nearestOctave = 11;
        for (int i = 0; i < nearestIxs.Length; i++)
        {
            int oct = Math.Abs(curOctave - octaves[i]);
            if (oct < nearestOctave)
            {
                nearestOctave = oct;
                nearestIx = nearestIxs[i];
            }
        }
        return haystack[nearestIx];
    }

    private static (int Delta, int[] Indexes, int[] Octaves) FindNearestDelta(int needle, int[] haystack)
    {
        int nearestDelta = 127;
        var nearestIxs = new List<int>();
        var octaves = new List<int>();

        for (int i = 0; i < haystack.Length; i++)
        {
            int needlePitch = needle % 12;
            int haystackPitch = haystack[i] % 12;
            int currentDelta = Math.Min(Math.Abs(needlePitch - haystackPitch), Math.Abs(needlePitch - haystackPitch - 12));
            if (currentDelta <= nearestDelta)
                nearestDelta = currentDelta;
        }
        for (int i = 0; i < haystack.Length; i++)
        {
            int needlePitch = needle % 12;
            int haystackPitch = haystack[i] % 12;
            int currentDelta = Math.Min(Math.Abs(needlePitch - haystackPitch), Math.Abs(needlePitch - haystackPitch - 12));
            if (currentDelta == nearestDelta)
            {
                nearestIxs.Add(i);
                octaves.Add(haystack[i] / 12);
            }
        }
        return (nearestDelta, nearestIxs.ToArray(), octaves.ToArray());
    }

    public static void NormalizeClipLengths(Clip[] clips)
    {
        if (clips.Length < 2) return;
        var newLength = clips.Max(x => x.Length);
        foreach (var clip in clips)
            EnlargeClipByLooping(clip, newLength);
    }

    public static void EnlargeClipByLooping(Clip clip, decimal newLength)
    {
        if (newLength < clip.Length) return;
        
        var loopLength = clip.Length;
        var currentLength = loopLength;
        var notesToAdd = new List<NoteEvent>();

        while (currentLength < newLength)
        {
            foreach (var note in clip.Notes)
            {
                if (note.Start + currentLength < newLength)
                {
                    var noteToAdd = new NoteEvent(note)
                    {
                        Start = note.Start + currentLength
                    };
                    if (note.End + currentLength > newLength)
                    {
                        noteToAdd.Duration = newLength - noteToAdd.Start;
                        // don't exit here as there might be more stacked/clustered notes that also fit
                    }
                    notesToAdd.Add(noteToAdd);
                } else {
                    break;
                }
            }
            currentLength += loopLength;
        }
        clip.Length = newLength;
        clip.Notes.AddRange(notesToAdd);
    }
    
    public static Clip Monophonize(Clip clip)
    {
        if (clip.Notes.Count < 2) return clip;
        var notesToRemove = new List<NoteEvent>();
        var result = new SortedList<NoteEvent>();

        var i = 0;
        foreach (var note in clip.Notes)
        {
            i++;
            if (notesToRemove.Contains(note)) continue;
            var overlappingNotes = clip.Notes.Skip(i).Where(x => x.StartsInsideIntervalInclusive(note.Start, note.End)).ToList();
            notesToRemove.AddRange(overlappingNotes);
        }
        foreach (var note in clip.Notes)
        {
            if (!notesToRemove.Contains(note))
            {
                result.Add(note);
            }
        }
        clip.Notes = result;
        return clip;
    }

    public static void AddNoteCutting(Clip clip, NoteEvent noteToAdd)
    {
        var collidingNotes = clip.Notes.Where(x => x.Pitch == noteToAdd.Pitch && noteToAdd.StartsInsideInterval(x.Start, x.End)).ToArray();
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
                    note.Duration = noteToAdd.Start - note.Start;
                }
            }
        }
        clip.Notes.Add(noteToAdd);
    }

    public static Clip[] CreateEmptyPlaceholderClips(IEnumerable<Clip> clips)
    {
        return clips.Select(x => new Clip(x.Length, x.IsLooping)).ToArray();
    }
}