using Mutate4l.Core;

namespace Mutate4l.Options
{
    public class ConstrainOptions
    {
        [OptionInfo(type: OptionType.AllOrSpecified)]
        public bool Pitch { get; set; }

        [OptionInfo(type: OptionType.AllOrSpecified)]
        public bool Start { get; set; }

        [OptionInfo(min: 1, max: 100)]
        public int Strength { get; set; } = 100;

        // todo: possibly have option for whether Constrain should output only processed clips or if source clip should also be included
        // todo: option for whether Strength should also affect Pitch?
    }
}
