using System;
using System.Collections.Generic;
using System.Linq;
using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.Utility;

namespace Mutate4l.Commands
{
    public class QuantizeOptions
    {
        public decimal Amount { get; set; } = 1;

        [OptionInfo(type: OptionType.Default)]
        public decimal[] Divisions { get; set; } = { 0.25m };

        public decimal Threshold { get; set; } = 0.125m;

        public decimal Magnetic { get; set; } = 0;
        
        public Clip By { get; set; } // for quantizing based on another clip (possibly very similar to constrain -start?)
    }
    
    public class Quantize
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out QuantizeOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(QuantizeOptions options, params Clip[] clips)
        {
            if (options.By != null)
            {
                var maxLen = clips.Max(x => x.Length);
                if (options.By.Length < maxLen)
                {
                    ClipUtilities.EnlargeClipByLooping(options.By, maxLen);
                }
                options.Divisions = options.By.Notes.Select(x => x.Start).Distinct().ToArray();
            }
            options.Amount = Math.Clamp(options.Amount, 0, 1);

            for (var i = 1; i < clips.Length; i++)
            {
                var clip = clips[i];
                
            }
        }


        /*
        var newStart = ClipUtilities.FindNearestNoteStartInSet(note, masterClip.Notes);
        constrainedNote.Start += (newStart - constrainedNote.Start) * (options.Strength / 100);
        */
    }
}