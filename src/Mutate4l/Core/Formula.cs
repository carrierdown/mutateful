using System.Collections.Generic;
using Mutate4l.Cli;

namespace Mutate4l.Core
{
    public class Formula
    {
        public List<Command> Commands { get; }
        public List<ClipReference> SourceClipReferences { get; }
        public List<ClipReference> AllReferencedClips { get; }
        
        public static readonly Formula Empty = new Formula();

        public Formula()
        {
            Commands = new List<Command>();
            SourceClipReferences = new List<ClipReference>();
            AllReferencedClips = new List<ClipReference>();
        }

        public Formula(List<Command> commands, List<ClipReference> sourceClipReferences, List<ClipReference> allReferencedClips)
        {
            Commands = commands;
            SourceClipReferences = sourceClipReferences;
            AllReferencedClips = allReferencedClips;
        }
    }
}