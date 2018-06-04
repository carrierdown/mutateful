using Mutate4l.Core;

namespace Mutate4l.Options
{
    public class ArpeggiateOptions
    {
        [OptionInfo(min: 1, max: 10)]
        public int Rescale { get; set; } = 2;

        public bool RemoveOffset = true;
    }
}
