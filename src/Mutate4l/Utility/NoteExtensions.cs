using System;
using Mutate4l.Dto;

namespace Mutate4l.Utility
{
    public static class NoteExtensions
    {
        //   [ ---    ]
        public static bool InsideInterval(this Note note, decimal start, decimal end)
        {
            return note.Start >= start && note.Start + note.Duration <= end;
        }

        // --[--------]---
        public static bool CoversInterval(this Note note, decimal start, decimal end)
        {
			return note.Start < start && note.Start + note.Duration > end;
        }

        // --[------  ]
        public static bool CrossesStartOfInterval(this Note note, decimal start, decimal end)
        {
            return note.Start < start && (note.Start + note.Duration) > start && (note.Start + note.Duration) <= end;
        }

        //   [    ----]----
        public static bool CrossesEndOfInterval(this Note note, decimal start, decimal end)
        {
            return note.Start >= start && (note.Start + note.Duration) > end;
        }
    }
}
