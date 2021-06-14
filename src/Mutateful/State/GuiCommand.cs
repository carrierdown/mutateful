using Mutateful.Core;

namespace Mutateful.State
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