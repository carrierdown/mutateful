using System;
using System.Collections.Generic;
using System.Text;
using Mutate4l.Core;

namespace Mutate4l.Dto
{
    public class NoteInfo
    {
        public int Pitch { get; set; }
        public decimal Start { get; set; }
        public decimal Duration { get; set; }
        public int Velocity { get; set; }
        public decimal DurationUntilNextNote { get; set; }
    }

    public class Note : IComparable<Note>, INoteEvent
    {
        public int Pitch { get; set; }
        public decimal Start { get; set; }
        public decimal Duration { get; set; }
        public int Velocity { get; set; }
        public decimal End => Start + Duration;

        public Note(int pitch, decimal start, decimal duration, int velocity)
        {
            Pitch = pitch;
            Start = start;
            Duration = duration;
            Velocity = velocity;
        }

        public Note(Note note)
        {
            Pitch = note.Pitch;
            Start = note.Start;
            Duration = note.Duration;
            Velocity = note.Velocity;
        }

        public int CompareTo(Note b)
        {
            if (Start < b.Start)
            {
                return -1;
            }
            if (Start > b.Start)
            {
                return 1;
            }
            if (Pitch < b.Pitch)
            {
                return -1;
            }
            if (Pitch > b.Pitch)
            {
                return 1;
            }
            return 0;
        }
    }
}
