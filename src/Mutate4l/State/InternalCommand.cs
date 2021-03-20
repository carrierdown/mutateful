using Mutate4l.Core;

namespace Mutate4l.State
{
    public enum InternalCommandType
    {
        UnknownCommand,
        OutputString,
        LegacyProcessClips,
        SetFormulaOnServer, // set a single clip slot without triggering evaluation
        SetClipDataOnServer, // set and trigger evaluation of a single clip slot
        SetAndEvaluateFormulaOnServer,
        SetAndEvaluateClipDataOnServer,
        EvaluateFormulas, // trigger evaluation of one or more clipslots - arguments are a list of clipslot coordinates
        SetFormulaOnClient,
        SetClipDataOnClient,
        SetClipDataOnClientLive11
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