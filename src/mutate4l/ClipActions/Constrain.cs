using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.ClipActions
{
    public class Constrain
    {
        public static bool ConstrainPitch;
        public static bool ConstrainStart;
        
        public static Clip Apply(Clip a, Clip b)
        {
            Utility.NormalizeClipLengths(a, b);
            Clip constrainedClip = new Clip(b, b.IsLooping);

            foreach (var note in b)
            {
                var constrainedNote = new Note(note);
                if (ConstrainPitch)
                {
                    constrainedNote.Pitch = Utility.FindNearestNotePitchInSet(note, a);
                }
                if (ConstrainStart)
                {
                    constrainedNote.Start = Utility.FindNearestNoteStartInSet(note, a);
                }
                constrainedClip.Notes.Add(constrainedNote);
            }
            return constrainedClip;
        }
    }
}
