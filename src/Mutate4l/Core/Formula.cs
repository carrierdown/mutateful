using System.Collections.Generic;
using Mutate4l.State;

namespace Mutate4l.Core
{
    public class Formula
    {
        public Dictionary<ClipReference, ClipSlot> ClipSlotsByClipReference { get; }
        public List<Command> Commands { get; }
        public List<ClipReference> SourceClipReferences { get; }
        public List<ClipReference> AllReferencedClips { get; }
        
        public static readonly Formula Empty = new Formula();

        public Formula() : this(new List<Command>(), new List<ClipReference>(), new List<ClipReference>(), new Dictionary<ClipReference, ClipSlot>()) { }

        public Formula(List<Command> commands, List<ClipReference> sourceClipReferences, List<ClipReference> allReferencedClips, Dictionary<ClipReference, ClipSlot> clipSlotsByClipReference)
        {
            Commands = commands;
            SourceClipReferences = sourceClipReferences;
            AllReferencedClips = allReferencedClips;
            ClipSlotsByClipReference = clipSlotsByClipReference;
        }
    }
}