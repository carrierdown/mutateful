using Mutate4l.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Options
{
    public class SliceOptions
    {
        [OptionInfo(type: OptionType.Default)]
        public decimal[] Lengths { get; set; } = new decimal[] { .25m };
    }
}
