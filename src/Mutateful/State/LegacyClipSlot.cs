namespace Mutateful.State;

public class LegacyClipSlot
{
    public Clip Clip { get; set; }
    public string Name { get; }
    public ChainedCommand ChainedCommand { get; }
    public ushort Id { get; }
    public ClipReference ClipReference => Clip.ClipReference;
    
    public static readonly LegacyClipSlot Empty = new LegacyClipSlot("", Clip.Empty, ChainedCommand.Empty, ushort.MaxValue);
    
    public LegacyClipSlot(string name, Clip clip, ChainedCommand chainedCommand, ushort id)
    {
        Name = name;
        Clip = clip;
        ChainedCommand = chainedCommand;
        Id = id;
    }        
}