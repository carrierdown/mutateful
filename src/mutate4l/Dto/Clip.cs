using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mutate4l.Dto
{
    public class Clip : IComparable<Clip>
    {
        private List<Note> _notes;
        public List<Note> Notes {
            get { return _notes; }
            set { _notes = value; _notes.Sort(); }
        } // todo: optimization potential
        public decimal Length { get; set; }
        public bool IsLooping { get; set; }
        public decimal EndDelta
        {
            get { return Length - Notes.Max().Start; }
        }

        public Clip(decimal length, bool isLooping)
        {
            _notes = new List<Note>();
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
