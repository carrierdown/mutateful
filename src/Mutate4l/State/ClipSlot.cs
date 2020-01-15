using Mutate4l.Core;

namespace Mutate4l.State
{
    public class ClipSlot
    {
        public Clip Clip { get; }
        
        public string Name { get; }
        
        public ChainedCommand ChainedCommand { get; }

        public static readonly ClipSlot Empty = new ClipSlot("", Clip.Empty, ChainedCommand.Empty);
        
        public ClipSlot(string name, Clip clip, ChainedCommand command)
        {
            Name = name;
            Clip = clip;
            ChainedCommand = command;
        }
    }
}