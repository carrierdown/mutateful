using Mutateful.Core;

namespace Mutateful.Commands.ToBeImplemented
{
    public class ShiftOptions
    {
        [OptionInfo(type:OptionType.Default)]
        public decimal[] Amounts { get; set; }
    }
    
    // Simple command to shift contents of clip a specified amount. Notes will be shifted unequally if more values are specified, such that shift values are cycled.
    public class Shift
    {
        
    }
}