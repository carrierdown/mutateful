using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Dto
{
    public class Clip
    {
        public List<Note> Notes { get; set; }
        public decimal Length { get; set; }
        public bool IsLooping { get; set; }

        public Clip(decimal length, bool isLooping)
        {
            Notes = new List<Note>();
            IsLooping = isLooping;
            Length = length;
        }
    }
}
