using Mutate4l.Dto;
using System;
using System.Collections.Generic;

namespace Mutate4l.ClipActions
{
    public class Utility
    {
        public static List<Note> SplitNotesAtEvery(List<Note> notes, decimal position, decimal length)
        {
            decimal currentPosition = 0;
            while (currentPosition < length)
            {
                int i = 0;
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
                currentPosition = currentPosition + position;
            }
            return notes;
        }

        public static List<Note> GetNotesInRangeAtPosition(decimal start, decimal end, List<Note> notes, decimal position)
        {
            var results = new List<Note>();
            var notesFromRange = GetNotesInRange(start, end, notes);

            foreach (var note in notesFromRange)
            {
                note.Start = position;
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

        public static byte FindNearestNotePitchInSet(Note needle, List<Note> haystack)
        {
            int nearestIndex = 0;
            byte? nearestDelta = null;

            for (int i = 0; i < haystack.Count; i++)
            {
                if (nearestDelta == null)
                {
                    nearestDelta = (byte)Math.Abs(needle.Pitch - haystack[i].Pitch);
                }
                byte currentDelta = (byte)Math.Abs(needle.Pitch - haystack[i].Pitch);
                if (currentDelta < nearestDelta)
                {
                    nearestDelta = currentDelta;
                    nearestIndex = i;
                }
            }
            return haystack[nearestIndex].Pitch;
        }
    }
}
