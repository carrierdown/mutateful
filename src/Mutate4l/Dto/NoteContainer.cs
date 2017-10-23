using Mutate4l.Core;
using Mutate4l.Dto;

namespace Mutate4l.Utility
{
    public class NoteContainer : INoteEvent
    {
        public decimal Start { get; private set; }
        public decimal End { get; private set; }
        public decimal Duration { get; private set; }
        public Note[] Notes { 
            get {
                return NotesField;
            } 
            set {
                // we only expect sorted lists of notes
                NotesField = value;
                if (NotesField?.Length > 0) {
                    Start = NotesField[0].Start;
                    Duration = NotesField[0].Duration;
                    End = NotesField[0].Start + Duration;

                    for (var i = 1; i < NotesField.Length; i++)
                    {
                        Duration = NotesField[i].Start - Start + NotesField[i].Duration;
                        End = Start + Duration;
                    }
                }
            }
        }
        private Note[] NotesField;

        public NoteContainer(Note[] notes)
        {
            Notes = notes;
        }
    }
}