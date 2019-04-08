using Mutate4l.Core;

namespace Mutate4l.Utility
{
    public static class NoteExtensions
    {
        //   [ ---    ]
        public static bool InsideInterval(this NoteEvent note, decimal start, decimal end)
        {
            return note.Start >= start && note.Start + note.Duration <= end;
        }

        // --[--------]---
        public static bool CoversInterval(this NoteEvent note, decimal start, decimal end)
        {
			return note.Start < start && note.Start + note.Duration > end;
        }

        // --[------  ]
        public static bool CrossesStartOfInterval(this NoteEvent note, decimal start, decimal end)
        {
            return note.Start < start && (note.Start + note.Duration) > start && (note.Start + note.Duration) <= end;
        }

        //   [    ----]----
        public static bool CrossesEndOfInterval(this NoteEvent note, decimal start, decimal end)
        {
            return note.Start >= start && (note.Start + note.Duration) > end;
        }

        //   [ -------]??
        public static bool StartsInsideInterval(this NoteEvent note, decimal start, decimal end)
        {
            return note.Start >= start && note.Start < end;
        }

    }
}
