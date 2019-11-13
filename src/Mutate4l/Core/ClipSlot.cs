namespace Mutate4l.Core
{
    public class ClipSlot
    {
        public Clip Clip { get; set; }
        
        public string Name { get; set; }

        public static ClipSlot EmptyField;
        
        public static ClipSlot Empty
        {
            get { return EmptyField ??= new ClipSlot("", new Clip(4, true)); }
        }

        public ClipSlot(string name, Clip clip)
        {
            Name = name;
            Clip = clip;
        }
    }
}