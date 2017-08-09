﻿using Mutate4l.Core;
using System;

namespace Mutate4l.Dto
{
    public class Clip : IComparable<Clip>
    {
        public SortedList<Note> Notes { get; set; }
        public decimal Length { get; set; }
        public bool IsLooping { get; set; }
        public decimal EndDelta
        {
            get { return Length - Notes[Notes.Count - 1].Start + Notes[0].Start; }
        }
        public bool Retriggered { get; private set; } = false;

        private int Index;

        public Clip(decimal length, bool isLooping)
        {
            Notes = new SortedList<Note>();
            IsLooping = isLooping;
            Length = length;
            Index = 0;
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

        public NoteInfo GetNextNoteInfo()
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
        }
    }
}
