using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using Mutate4l.Cli;
using static Mutate4l.Cli.TokenType;
using System.Linq;

namespace Mutate4l.ClipActions
{
    public class ConstrainOptions
    {
        public bool Pitch { get; set; }
        public bool Start { get; set; }
    }

    // constrain = constrain pitch:true start:true, constrain pitch = constrain pitch:true start:false, similarly for constrain start
    public class Constrain : IClipAction // todo: interface not really needed
    {
        public bool ConstrainPitch;
        public bool ConstrainStart;

        public Constrain(Dictionary<TokenType, List<string>> options)
        {
            // todo: maybe generalize this logic more - maybe using a map with a specific structure, option types, and a function to parse and validate against it.
            // e.g. OptionMap -> OptionGroup -> Option
            // Apply function then checks Option object directly instead of having to maintain/check class fields.
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

        public ProcessResult Apply(params Clip[] clips)
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
            return new ProcessResult(new Clip[] { constrainedClip });
        }
    }
}
