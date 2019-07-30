using System;
using System.Linq;
using Mutate4l.Core;

namespace Mutate4l.Utility
{
    public static class SvgUtilities
    {
        private static int PianoRollWidth { get; } = 30;

        public static string SvgFromClip(Clip clip, int x, int y, int width, int height)
        {
            var highestNote = Math.Clamp(clip.Notes.Max(c => c.Pitch) + 4, 0, 127); // Leave 3 notes on each side
            var lowestNote = Math.Clamp(clip.Notes.Min(c => c.Pitch) - 3, 0, 127); // as padding
            var numNotes = highestNote - lowestNote;
            
            var output = $"<svg version=\"1.1\" baseProfile=\"full\" width=\"{width}\" height=\"{height}\" xmlns=\"http://www.w3.org/2000/svg\">" +
                         $"<rect style=\"fill:#dddddd;stroke-width:0\" x=\"{PianoRollWidth}\" y=\"{y}\" width=\"{width - PianoRollWidth}\" height=\"{height}\" />";
            
            var yDelta = (decimal)height / numNotes;
            // piano + horizontal guides
            var j = 0;
            for (var i = lowestNote; i <= highestNote; i++)
            {
                var pianoColour = (i % 12 == 0 || i % 12 == 2 || i % 12 == 4 || i % 12 == 5 || i % 12 == 7 || i % 12 == 9 || i % 12 == 11) == true ? "ffffff" : "000000";
                output += $"<rect style=\"fill:#{pianoColour};stroke:#8e8e8e;stroke-width:1;stroke-miterlimit:4\" x=\"{x}\" y=\"{y + height - yDelta - (j * yDelta)}\" width=\"30\" height=\"{yDelta}\" />";
                if (i % 12 == 1 || i % 12 == 3 || i % 12 == 6 || i % 12 == 8 || i % 12 == 10)
                {
                    output += $"<rect style=\"fill:#cccccc;stroke-width:0;\" x=\"{PianoRollWidth}\" y=\"{y + height - yDelta - (j * yDelta)}\" width=\"{width - PianoRollWidth}\" height=\"{yDelta}\" />";
                }
                output += $"<line x1=\"{x + PianoRollWidth}\" x2=\"{x + width}\" y1=\"{y + height - yDelta - (j * yDelta)}\" y2=\"{y + height - yDelta - (j * yDelta)}\" stroke-width=\"1\" stroke=\"#bbbbbb\" />";
                j++;
            }
            // vertical guides
            var xDelta = (width - PianoRollWidth) / clip.Length;
            for (decimal i = 0; i < clip.Length; i += .25m) 
            {
                output += $"<line x1=\"{x + PianoRollWidth + (i * xDelta)}\" x2=\"{x + PianoRollWidth + (i * xDelta)}\" y1=\"{y}\" y2=\"{y + height}\" stroke-width=\"1\" stroke=\"#e6e6e6\" />";
            }
/*
            for (decimal i = .25m; i < clip.Length; i += .5m) // 8ths for now
            {
                output += $"<line x1=\"{x + PianoRollWidth + (i * xDelta)}\" x2=\"{x + PianoRollWidth + (i * xDelta)}\" y1=\"{y}\" y2=\"{y + height}\" stroke-width=\"1\" stroke=\"#eeeeee\" />";
            }
*/
            foreach (var note in clip.Notes)
            {
                if (note.Pitch >= lowestNote && note.Pitch <= highestNote)
                {
                    output += $"<rect style=\"fill:#ebebbc;stroke:#8e8e8e;stroke-width:0.5;stroke-miterlimit:4\" x=\"{x + PianoRollWidth + (note.Start * xDelta)}\" y=\"{y + (highestNote - note.Pitch - 1) * yDelta}\" width=\"{note.Duration * xDelta}\" height=\"{yDelta}\" />";
                }
            }
            output += "</svg>";
            return output;
        }
    }
}
