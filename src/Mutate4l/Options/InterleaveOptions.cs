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
        public bool Mask { get; set; } = false; // Instead of vvv xxx=vxvxvx, the current input "masks" the corresponding location of other inputs, producing vxv instead. Rename skip maybe?
        public bool ChunkChords { get; set; } = true; // Process notes having the exact same start times as a single event.
        public int[] EnableMask { get; set; } = new int[] { 1 }; // Allows specifying a sequence of numbers to use as a mask for whether the note should be included or omitted. E.g. 1 0 will alternately play and omit every even/odd note. Useful when combining two or more clips but you want to retain only the notes for the current track. In this scenario you would have several formulas that are the same except having different masks.
    }
}
