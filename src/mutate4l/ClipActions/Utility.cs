using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.ClipActions
{
    public class Utility
    {
        public static List<Note> SplitNotesAtEvery(List<Note> notes, decimal position, decimal length)
        {
            int i = 0;
            decimal currentPosition = 0;
            while (currentPosition < length)
            {
                while (i < notes.Count)
                {
                    Note note = notes[i];
                    if (note.Start > currentPosition) break;
                    if (note.Start < currentPosition && (note.Start + note.Duration) > currentPosition)
                    {
                        // note runs across range boundary - split it
                        decimal rightSplitDuration = note.Start + note.Duration - currentPosition;
                        note.Duration = currentPosition - note.Start;
                        Note newNote = new Note(note.Pitch, currentPosition, rightSplitDuration, note.Velocity);
                        notes.Insert(i + 1, newNote);
                        i++;
                    }
                    i++;
                }
                currentPosition = currentPosition.plus(position);
            }
            return notes;
        }

        public static List<Note> GetNotesInRangeAtPosition(decimal start, decimal end, List<Note> notes, decimal position)
        {
            var results = new List<Note>();
            var notesFromRange = GetNotesInRange(start, end, notes);

            foreach (var note in notesFromRange)
            {
                note.Start = note.Start + position - start;
            }
            results.AddRange(notesFromRange);
            return results;
        }

        public static List<Note> GetNotesInRange(decimal start, decimal end, List<Note> notes)
        {
            var results = new List<Note>();

            foreach (var note in notes)
            {
                if (note.Start > end)
                {
                    break;
                }
                if (note.Start >= start && note.Start < end)
                {
                    results.Add(new Note(note.Pitch, note.Start, note.Duration, note.Velocity));
                }
            }
            return results;
        }

        public static decimal FindNearestNoteStartInSet(Note needle, List<Note> haystack)
        {
            var nearestIndex = 0;
            decimal? nearestDelta = null;

            for (int i = 0; i < haystack.Count; i++)
            {
                if (nearestDelta == null)
                {
                    nearestDelta = needle.Start - Math.Abs(haystack[i].Start);
                }
                decimal currentDelta = needle.Start - Math.Abs(haystack[i].Start);
                if (currentDelta < nearestDelta)
                {
                    nearestDelta = currentDelta;
                    nearestIndex = i;
                }
            }
            return haystack[nearestIndex].Start;
        }

        public static findNearestNotePitchInSet(needle: Note, haystack: Note[]): number {
        var nearestIndex: number = 0,
            nearestDelta: number;
        for (let i = 0; i<haystack.length; i++) {
            if (nearestDelta === undefined) {
                nearestDelta = Math.abs(needle.getPitch() - haystack[i].getPitch());
            }
            let currentDelta: number = Math.abs(needle.getPitch() - haystack[i].getPitch());
            if (currentDelta<nearestDelta) {
                nearestDelta = currentDelta;
                nearestIndex = i;
            }
        }
        return haystack[nearestIndex].getPitch();
    }

    // sorts notes according to position
    public static sortNotes(notes: Note[]): void {
        notes = notes.sort((a: Note, b: Note) => {
            if (a.getStart().lt(b.getStart())) {
                return -1;
            }
            if (a.getStart().gt(b.getStart())) {
                return 1;
            }
            return 0;
        });
    }
    }
}
