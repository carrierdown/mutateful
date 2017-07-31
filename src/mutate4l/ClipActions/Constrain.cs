using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using Mutate4l.Cli;
using static Mutate4l.Cli.TokenType;
using System.Linq;
using Mutate4l.Core;

namespace Mutate4l.ClipActions
{
    public class ConstrainOptions
    {
        [OptionInfo(type: OptionType.InverseToggle)]
        public bool Pitch { get; set; }

        [OptionInfo(type: OptionType.InverseToggle)]
        public bool Start { get; set; }
    }

    // constrain = constrain pitch:true start:true, constrain pitch = constrain pitch:true start:false, similarly for constrain start
    public class Constrain
    {
        public static ProcessResult Apply(ConstrainOptions options, params Clip[] clips)
        {
            if (clips.Length < 2)
            {
                return new ProcessResult("Error: Less than two clips were specified.");
            }
            // TODO: Add support for more than two clips
            Clip a = clips[0];
            Clip b = clips[1];
            Utility.NormalizeClipLengths(a, b);
            Clip constrainedClip = new Clip(b.Length, b.IsLooping);

            foreach (var note in b.Notes)
            {
                var constrainedNote = new Note(note);
                if (options.Pitch)
                {
                    constrainedNote.Pitch = Utility.FindNearestNotePitchInSet(note, a.Notes);
                }
                if (options.Start)
                {
                    constrainedNote.Start = Utility.FindNearestNoteStartInSet(note, a.Notes);
                }
                constrainedClip.Notes.Add(constrainedNote);
            }
            return new ProcessResult(new Clip[] { constrainedClip });
        }
    }
}
