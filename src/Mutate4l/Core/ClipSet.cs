using System;
using System.Collections.Generic;

namespace Mutate4l.Core
{
    public enum InternalCommandType
    {
        SetName,
        Delete
    }

    public class InternalCommand
    {
        public InternalCommandType Type { get; }
        public ClipReference ClipReference { get; }
        public byte[] Payload { get; }
    }
    
    public class ClipSet
    {
        private Dictionary<ClipReference, ClipSlot> ClipSlots { get; }

        public ClipSlot this[ClipReference clipRef] => ClipSlots[clipRef];

        public void ApplyCommand(InternalCommand command)
        {
            switch (command.Type)
            {
                case InternalCommandType.Delete:
                    // add this command to the undo stack with type and state prior to deletion
                    if (ClipSlots.ContainsKey(command.ClipReference))
                    {
                        ClipSlots.Remove(command.ClipReference);
                    }
                    break;
                case InternalCommandType.SetName:
                    break;
            }
        }
        
        // something like a List of InternalCommands and previous state ClipSlots could be used to support undo/redo
    }
}