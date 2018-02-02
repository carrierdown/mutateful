using Mutate4l.Core;

namespace Mutate4l.Options
{
    public class RatchetOptions
    {
        [OptionInfo(min: 1, max: 20)]
        public int Min { get; set; } = 1;

        [OptionInfo(min: 1, max: 20)]
        public int Max { get; set; } = 3;

        public decimal[] Shape { get; set; } = new decimal[] { .5m, .5m };
    }
}