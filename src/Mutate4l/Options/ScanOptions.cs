using Mutate4l.Core;

namespace Mutate4l.Options
{
    public class ScanOptions
    {
        public decimal Window { get; set; } = 1;

        [OptionInfo(min: 1, max: 500)]
        public int Count { get; set; } = 8;
    }
}
