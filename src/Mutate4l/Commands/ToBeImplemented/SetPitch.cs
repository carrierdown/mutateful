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
    public class SetPitch
    {
        
    }
}