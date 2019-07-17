using Mutate4l.Core;

namespace Mutate4l.Commands
{
    public class QuantizeOptions
    {
        public decimal Amount { get; set; } = 1;

        [OptionInfo(type: OptionType.Default)]
        public decimal Division { get; set; } = 0.25m;

        public decimal Threshold { get; set; } = 0.125m;

        public decimal Magnetic { get; set; } = 0;
    }
    
    public class Quantize
    {
        
    }
}