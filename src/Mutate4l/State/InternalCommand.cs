using Mutate4l.Core;

namespace Mutate4l.State
{
    public enum InternalCommandType
    {
        UnknownCommand,
        OutputString,
        LegacyProcessClips,
        MutatefulSetFormula, // set a single clip slot without triggering evaluation
        MutatefulSetClipData, // set and trigger evaluation of a single clip slot
        EvaluateFormulas, // trigger evaluation of one or more clipslots - arguments are a list of clipslot coordinates
        LiveSetFormula,
        LiveSetClipData,
    }
    
    public class InternalCommand
    {
        public InternalCommandType Type { get; }
        public ClipSlot ClipSlot { get; }
        public ClipReference[] ClipReferences { get; }

        public InternalCommand(InternalCommandType type, ClipSlot clipSlot, ClipReference[] clipReferences = null)
        {
            Type = type;
            ClipSlot = clipSlot;
            ClipReferences = clipReferences ?? new ClipReference[0];
        }
    }
}