using static Mutate4l.Options.InterleaveMode;

namespace Mutate4l.Options
{
    public enum InterleaveMode
    {
        Event,
        Time
    }

    public class InterleaveOptions
    {
        public InterleaveMode Mode { get; set; } = Time;
        public int[] Repeats { get; set; } = new int[] { 1 };
        public decimal[] Ranges { get; set; } = new decimal[] { 1 };
        public bool Mask { get; set; } = false; // Instead of vvv xxx=vxvxvx, the current input "masks" the corresponding location of other inputs, producing vxv instead.
    }
}
