using Mutate4l.Core;
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
            get { return Length - Notes[Notes.Count - 1].Start; }
        }

        public Clip(decimal length, bool isLooping)
        {
            Notes = new SortedList<Note>();
            IsLooping = isLooping;
            Length = length;
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
    }
}
