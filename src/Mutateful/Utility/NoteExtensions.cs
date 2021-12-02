namespace Mutateful.Utility;

public static class NoteExtensions
{
    //   [ ---    ]
    public static bool InsideIntervalInclusive(this NoteEvent note, decimal start, decimal end)
    {
        return note.Start >= start && note.Start + note.Duration <= end;
    }

    // --[--------]---
    public static bool CoversInterval(this NoteEvent note, decimal start, decimal end)
    {
		return note.Start < start && note.Start + note.Duration > end;
    }

    // --[------  ]
    public static bool CrossesStartOfIntervalInclusive(this NoteEvent note, decimal start, decimal end)
    {
        return note.Start < start && (note.Start + note.Duration) > start && (note.Start + note.Duration) <= end;
    }

    //   [    ----]----
    public static bool CrossesEndOfIntervalInclusive(this NoteEvent note, decimal start, decimal end)
    {
        return note.Start >= start && note.Start < end && (note.Start + note.Duration) > end;
    }

    //   [--------]??
    public static bool StartsInsideIntervalInclusive(this NoteEvent note, decimal start, decimal end)
    {
        return note.Start >= start && note.Start < end;
    }

    public static bool StartsInsideInterval(this NoteEvent note, decimal start, decimal end)
    {
        return note.Start > start && note.Start < end;
    }
}