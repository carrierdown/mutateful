using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Dto
{
    public class NoteInfo
    {
        public byte Pitch { get; set; }
        public decimal Start { get; set; }
        public decimal Duration { get; set; }
        public byte Velocity { get; set; }
        public decimal DurationUntilNextNote { get; set; }
    }

    public class Note : IComparable<Note>
    {
        public byte Pitch { get; set; }
        public decimal Start { get; set; }
        public decimal Duration { get; set; }
        public byte Velocity { get; set; }

        public Note(byte pitch, decimal start, decimal duration, byte velocity)
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
