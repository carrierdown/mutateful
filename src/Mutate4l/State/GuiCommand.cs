using Mutate4l.Core;

namespace Mutate4l.State
{
    public enum GuiCommandType
    {
        SetName,
        Delete,
        UpdateFormula
    }
    
    public class GuiCommand
    {
        public GuiCommandType Type { get; }
        public ClipReference ClipReference { get; }
        public ChainedCommand Command { get; }
    }
}