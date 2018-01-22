using Mutate4l.Core;
using System;

namespace Mutate4l.Dto
{
    public class Clip : IComparable<Clip>
    {
        public SortedList<NoteEvent> Notes { get; set; }
        public decimal Length { get; set; }
        public bool IsLooping { get; set; }
        public decimal EndDelta
        {
            get { return Length - Notes[Notes.Count - 1].Start + Notes[0].Start; }
        }
        public bool SelectionActive { get; private set; }
        public bool ContainsChunks { get; private set; }

        // creates chunk by adding note event 2..n as children to note event 1, removing them from the Notes list in the process
        public void Chunkify(params NoteEvent[] noteEvents)
        {
            if (noteEvents.Length < 2) return;
            ContainsChunks = true;

        }

        // flattens clip by moving all child note events up to the master Notes list.
        public void Flatten()
        {
            if (!ContainsChunks) return;

        }

        public Clip(decimal length, bool isLooping)
        {
            Notes = new SortedList<NoteEvent>();
            IsLooping = isLooping;
            Length = length;
        }

        public Clip(Clip clip) : this(clip.Length, clip.IsLooping)
        {
            foreach (var note in clip.Notes)
            {
                var clonedNote = new NoteEvent(note);
                Notes.Add(clonedNote);
            }

        }

        public int CompareTo(Clip b)
        {
            if (Length < b.Length)
            {
                return -1;
            }
            if (Length > b.Length)
            {
                return 1;
            }
            return 0;
        }

        public decimal DurationUntilNextNote(int index)
        {
            // todo: warn if index > Notes.Count - 1
            if (index >= Notes.Count - 1)
                return EndDelta;
            else
                return Notes[index + 1].Start - Notes[index].Start;
        }

        // returns pitch relative to first note of clip
        public int RelativePitch(int index)
        {
            if (index < 0 || index >= Notes.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            return Notes[index].Pitch - Notes[0].Pitch;
        }

        /*        public NoteInfo GetNextNoteInfo()
                {
                    Note note;
                    if (Index == Notes.Count - 1)
                    {
                        note = Notes[Index];
                        Index = 0;
                        Retriggered = true;
                        return new NoteInfo { Start = note.Start, Duration = note.Duration, Pitch = note.Pitch, Velocity = note.Velocity, DurationUntilNextNote = EndDelta };
                    }
                    note = Notes[Index];
                    var result = new NoteInfo { Start = note.Start, Duration = note.Duration, Pitch = note.Pitch, Velocity = note.Velocity, DurationUntilNextNote = Notes[Index + 1].Start - Notes[Index].Start };
                    Index++;
                    return result;
                }*/
    }
}
