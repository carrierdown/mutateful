using Mutate4l.Core;

namespace Mutate4l.State
{
    public enum InternalCommandType
    {
        UnknownCommand,
        OutputString,
        SetClipSlot, // set a single clip slot without triggering evaluation
        SetAndEvaluateClipSlot, // set and trigger evaluation of a single clip slot
        EvaluateClipSlots, // trigger evaluation of one or more clipslots - arguments are a list of clipslot coordinates
    }
    
    public class InternalCommand
    {
        public InternalCommandType Type { get; }
        public ClipSlot ClipSlot { get; }
        public ClipReference[] ClipReferences { get; }

        public InternalCommand(InternalCommandType type, ClipSlot clipSlot, ClipReference[] clipReferences)
        {
            Type = type;
            ClipSlot = clipSlot;
            ClipReferences = clipReferences;
        }
    }
}