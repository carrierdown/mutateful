namespace Mutate4l.Core
{
    public class ClipSlot
    {
        public Clip Clip { get; }
        
        public string Name { get; }
        
        public ChainedCommand ChainedCommand { get; }

        private static ClipSlot EmptyField;
        
        public static ClipSlot Empty
        {
            get { return EmptyField ??= new ClipSlot("", Clip.Empty, ChainedCommand.Empty); }
        }

        public ClipSlot(string name, Clip clip, ChainedCommand command)
        {
            Name = name;
            Clip = clip;
            ChainedCommand = command;
        }
    }
}