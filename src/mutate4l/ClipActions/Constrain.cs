using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using Mutate4l.Cli;
using static Mutate4l.Cli.TokenType;
using System.Linq;

namespace Mutate4l.ClipActions
{
    // constrain = constrain pitch:true start:true, constrain pitch = constrain pitch:true start:false, similarly for constrain start
    public class Constrain : IClipAction
    {
        public bool ConstrainPitch;
        public bool ConstrainStart;

        public Constrain(Dictionary<TokenType, List<string>> options)
        {
            var validTokens = new TokenType[] { Start, Pitch };
            var validOptions = Utility.GetValidOptions(options, validTokens);

            if (validOptions.Keys.Count == 0 || options.Keys.Count == validTokens.Length)
            {
                ConstrainStart = true;
                ConstrainPitch = true;
            }
            else
            {
                foreach (var option in validOptions.Keys)
                {
                    if (option == Start) ConstrainStart = true;
                    if (option == Pitch) ConstrainPitch = true;
                }
            }
        }

        public Clip Apply(Clip a, Clip b)
        {
            Utility.NormalizeClipLengths(a, b);
            Clip constrainedClip = new Clip(b.Length, b.IsLooping);

            foreach (var note in b.Notes)
            {
                var constrainedNote = new Note(note);
                if (ConstrainPitch)
                {
                    constrainedNote.Pitch = Utility.FindNearestNotePitchInSet(note, a.Notes);
                }
                if (ConstrainStart)
                {
                    constrainedNote.Start = Utility.FindNearestNoteStartInSet(note, a.Notes);
                }
                constrainedClip.Notes.Add(constrainedNote);
            }
            return constrainedClip;
        }
    }
}
