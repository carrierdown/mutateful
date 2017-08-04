using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutate4l.ClipActions
{
    public class Utility
    {
        public static SortedList<Note> SplitNotesAtEvery(SortedList<Note> notes, decimal position, decimal length)
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
                        notes.Add(new Note(note.Pitch, currentPosition, rightSplitDuration, note.Velocity));
                        // For regular list: (though possible bug if split note spans more than one range unit)
                        //Note newNote = new Note(note.Pitch, currentPosition, rightSplitDuration, note.Velocity);
                        //notes.Insert(i + 1, newNote);
                        //i++;
                    }
                    i++;
                }
                currentPosition = currentPosition + position;
            }
            return notes;
        }

        public static List<Note> GetNotesInRangeAtPosition(decimal start, decimal end, SortedList<Note> notes, decimal position)
        {
            var results = new List<Note>();
            var notesFromRange = GetNotesInRange(start, end, notes);

            foreach (var note in notesFromRange)
            {
                note.Start += position;
            }
            results.AddRange(notesFromRange);
            return results;
        }

        public static List<Note> GetNotesInRange(decimal start, decimal end, SortedList<Note> notes)
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

        public static decimal FindNearestNoteStartInSet(Note needle, SortedList<Note> haystack)
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

        public static byte FindNearestNotePitchInSet(Note needle, SortedList<Note> haystack)
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

        public static void NormalizeClipLengths(params Clip[] clips)
        {
            decimal maxLength = clips.Max().Length;
            foreach (var clip in clips)
            {
                if (clip.Length < maxLength)
                {
                    decimal loopLength = clip.Length;
                    decimal currentLength = loopLength;

                    var notesToAdd = new List<Note>();
                    while (currentLength < maxLength)
                    {
                        foreach (var note in clip.Notes)
                        {
                            if (note.Start + currentLength < maxLength)
                            {
                                notesToAdd.Add(new Note(note)
                                {
                                    Start = note.Start + currentLength
                                });
                            } else {
                                break;
                            }
                        }
                        currentLength += loopLength;
                    }
                    clip.Length = maxLength;
                    clip.Notes.AddRange(notesToAdd);
                }
            }
        }

        public static Dictionary<TokenType, List<Token>> GetValidOptions(Dictionary<TokenType, List<Token>> options, TokenType[] validOptions)
        {
            var cleanedOptions = new Dictionary<TokenType, List<Token>>();
            foreach (var key in options.Keys)
            {
                if (validOptions.Contains(key))
                {
                    cleanedOptions.Add(key, options[key]);
                }
            }
            return cleanedOptions;
        }

        public static decimal MusicalDivisionToDecimal(string value)
        {
            if (value.IndexOf('/') >= 0)
            {
                return 4m / int.Parse(value.Substring(value.IndexOf('/') + 1));
            }
            else
            {
                return int.Parse(value) * 4m;
            }
        }
    }
}
