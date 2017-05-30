using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l
{
    public class Note
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
    }
}
