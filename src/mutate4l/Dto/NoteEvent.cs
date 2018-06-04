using System;
using System.Collections.Generic;
using System.Text;
using Mutate4l.Core;

namespace Mutate4l.Dto
{
    public class NoteEvent : IComparable<NoteEvent>, IEquatable<NoteEvent>
    {
        private int PitchField;
        private int VelocityField;

        public int Pitch { get { return Math.Clamp(PitchField, 0, 127); } set { PitchField = value; } }
        public decimal Start { get; set; }
        public decimal Duration { get; set; }
        public int Velocity { get { return Math.Clamp(VelocityField, 1, 127); } set { VelocityField = value; } }
        public decimal End => Start + Duration;
        public bool IsSelected { get; set; }
        public List<NoteEvent> Children { get; set; } // children have position, pitch and velocity relative to the parent noteEvent

        public NoteEvent(int pitch, decimal start, decimal duration, int velocity)
        {
            Pitch = pitch;
            Start = start;
            Duration = duration;
            Velocity = velocity;
        }

        public NoteEvent(NoteEvent note)
        {
            Pitch = note.Pitch;
            Start = note.Start;
            Duration = note.Duration;
            Velocity = note.Velocity;
            // todo: should also clone children
        }

        public int CompareTo(NoteEvent b)
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

        public bool Equals(NoteEvent other)
        {
            // We don't consider velocity or duration as only one note can occupy a specific start time and pitch
            return Start == other.Start && Pitch == other.Pitch;
        }
    }
}
