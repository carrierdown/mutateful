using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutate4l.Utility
{
    public class ClipUtilities
    {
        public static Clip SplitNotesAtEvery(Clip clip, decimal[] timespans)
        {
            decimal currentPosition = 0;
            int timespanIx = 0;
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
                if ((note.Start + note.Duration) < start) continue;
                if (note.Start > end) break;
                if (note.InsideInterval(start, end))
                {
                    AddNote(new NoteEvent(note.Pitch, note.Start - start + position, note.Duration, note.Velocity), results);
                }
                else if (note.CoversInterval(start, end))
                {
                    AddNote(new NoteEvent(note.Pitch, position, end - start, note.Velocity), results);
                }
                else if (note.CrossesStartOfInterval(start, end))
                {
                    AddNote(new NoteEvent(note.Pitch, position, (note.Start + note.Duration) - start, note.Velocity), results);
                }
                else if (note.CrossesEndOfInterval(start, end)) {
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

        public static void NormalizeClipLengths(params Clip[] clips)
        {
            decimal maxLength = clips.Max().Length;
            foreach (var clip in clips)
            {
                if (clip.Length < maxLength)
                {
                    decimal loopLength = clip.Length;
                    decimal currentLength = loopLength;

                    var notesToAdd = new List<NoteEvent>();
                    while (currentLength < maxLength)
                    {
                        foreach (var note in clip.Notes)
                        {
                            if (note.Start + currentLength < maxLength)
                            {
                                notesToAdd.Add(new NoteEvent(note)
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
                var overlappingNotes = clip.Notes.Skip(i).Where(x => x.StartsInsideInterval(note.Start, note.End)).ToList();
                foreach (var overlappingNote in overlappingNotes)
                {
                    notesToRemove.Add(overlappingNote);
                }
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
    }
}
