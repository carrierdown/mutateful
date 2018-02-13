using Mutate4l.Core;

namespace Mutate4l.Options
{
    public class RatchetOptions
    {
        [OptionInfo(min: 1, max: 20)]
        public int Min { get; set; } = 1;

        [OptionInfo(min: 1, max: 20)]
        public int Max { get; set; } = 8;

        [OptionInfo(min: 0, max: 100)]
        public int Strength { get; set; } = 100;

        public bool VelocityToStrength { get; set; }

        public decimal[] Shape { get; set; } = new decimal[] { .5m, .5m };

        // Automatically scale control sequence so that lowest note corresponds to minimum ratchet value and highest note corresponds to maximum ratchet value
        public bool AutoScale { get; set; }

        public int ControlMin { get; set; } = 60; // default lowest pitch for control sequence (unless AutoScale is on), e.g. pitch values 60 or lower equal Min ratchet-value.

        public int ControlMax { get; set; } = 68; // default highest pitch for control sequence (unless AutoScale is on), e.g. pitch values 72 or higher equal Max ratchet-value.
    }
}