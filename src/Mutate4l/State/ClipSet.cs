using System.Collections.Generic;
using Mutate4l.Core;

namespace Mutate4l.State
{
    public class ClipSet
    {
        private Dictionary<ClipReference, ClipSlot> ClipSlots { get; } = new Dictionary<ClipReference, ClipSlot>();

        private ClipReference InternalClipRef = new ClipReference(1, 1);
        public ClipSlot this[ClipReference clipRef] => ClipSlots[clipRef] ?? ClipSlot.Empty;
        
        public ClipSlot this[int track, int clip]
        {
            get
            {
                InternalClipRef.Track = track;
                InternalClipRef.Clip = clip;
                return ClipSlots[InternalClipRef]; 
            }
        }

        // Placeholder for future use
        public void ApplyGuiCommand(GuiCommand command)
        {
            switch (command.Type)
            {
                case GuiCommandType.Delete:
                    // add this command to the undo stack with type and state prior to deletion
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
                case InternalCommandType.SetFormula:
                    ClipSlots[command.ClipSlot.ClipReference] = command.ClipSlot;
                    break;
                case InternalCommandType.SetClipData:
                    ClipSlots[command.ClipSlot.ClipReference] = command.ClipSlot;
                    break;
            }
        }
    }
}