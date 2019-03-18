using Mutate4l.Core;
using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Commands
{
    public class ResizeOptions
    {
        [OptionInfo(type: OptionType.Default)]
        public decimal Factor { get; set; } = 1.0m;
    }

    public class Resize
    {
        public static ProcessResultArray<Clip> Apply(ResizeOptions options, params Clip[] clips)
        {
            foreach (var clip in clips)
            {
                foreach (var note in clip.Notes)
                {
                    note.Duration *= options.Factor;
                    note.Start *= options.Factor;
                }
                clip.Length *= options.Factor;
            }

            return new ProcessResultArray<Clip>(clips);
        }
    }
}
