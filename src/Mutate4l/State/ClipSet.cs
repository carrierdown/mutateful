using System.Collections.Generic;
using Mutate4l.Core;

namespace Mutate4l.State
{
    public class ClipSet
    {
        private Dictionary<ClipReference, ClipSlot> ClipSlots { get; }

        public ClipSlot this[ClipReference clipRef] => ClipSlots[clipRef] ?? ClipSlot.Empty;
        
        // Placeholder for future use
        public void ApplyGuiCommand(GuiCommand command)
        {
            switch (command.Type)
            {
                case GuiCommandType.Delete:
                    // add this command to the undo stack with type and state prior to deletion
                    if (ClipSlots.ContainsKey(command.ClipReference))
                    {
                        ClipSlots.Remove(command.ClipReference);
                    }
                    break;
                case GuiCommandType.SetName:
                    break;
            }
            // something like a List of InternalCommands and previous state ClipSlots could be used to support undo/redo
        }

        public void ApplyInternalCommand(InternalCommand command)
        {
            switch (command.Type)
            {
                case InternalCommandType.SetClipSlot:
                    
                    break;
            }
        }
    }
}