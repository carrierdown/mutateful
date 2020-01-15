using Mutate4l.Core;

namespace Mutate4l.State
{
    public enum InternalCommandType
    {
        SetName,
        Delete,
        SetClipSlot
        // UpdateFormula
    }
    
    public class InternalCommand
    {
        public InternalCommandType Type { get; }
        public ClipReference ClipReference { get; }
        public byte[] Payload { get; }
    }
}