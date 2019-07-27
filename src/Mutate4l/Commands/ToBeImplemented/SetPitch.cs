using Mutate4l.Core;

namespace Mutate4l.Commands.ToBeImplemented
{
    public class SetPitchOptions
    {
        [OptionInfo(0, 127, type:OptionType.Default)]
        public int[] Pitches { get; set; }
        
        public Clip By { get; set; }
    }
    
    // Simple command to set pitch of all notes to specified value(s). If more values are specified, they are cycled through.
    // Note: Kind of inelegant to just specify pitches using numbers. However, there is an issue supporting notes at present as
    // they are hard to distinguish from clip references (i.e. C1, A2, etc.). Will have to think about this one.
    public class SetPitch
    {
        
    }
}