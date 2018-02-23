using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Utility
{
    public class SvgUtilities
    {
        public static int PianoRollWidth { get; } = 30;

        public static string SvgFromClip(Clip clip, int x, int y, int width, int height, int octaves, int startNote)
        {
            var output = $"<svg version=\"1.1\" baseProfile=\"full\" width=\"{width}\" height=\"{height}\" xmlns=\"http://www.w3.org/2000/svg\">";
            var yDelta = height / (octaves * 12);
            // piano + horizontal guides
            for (int i = 0; i <= octaves * 12; i++)
            {
                bool white = i % 12 == 0 || i % 12 == 2 || i % 12 == 4 || i % 12 == 5 || i % 12 == 7 || i % 12 == 9 || i % 12 == 11;
                output += $"<rect style=\"fill:#{(white ? "ffffff" : "000000")};fill-opacity:1;stroke:#8e8e8e;stroke-width:1;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1\" x=\"0\" y=\"{300 - yDelta - (i * yDelta)}\" width=\"30\" height=\"{yDelta}\" />";
                output += $"<line x1=\"{PianoRollWidth}\" x2=\"{width}\" y1=\"{300 - yDelta - (i * yDelta)}\" y2=\"{300 - yDelta - (i * yDelta)}\" stroke-width=\"1\" stroke=\"#bbbbbb\" />";
            }
            // vertical guides
            var xDelta = (width - PianoRollWidth) / clip.Length;
            for (decimal i = 0; i < clip.Length; i += 4m / 8) // 8ths for now
            {
                output += $"<line x1=\"{PianoRollWidth + (i * xDelta)}\" x2=\"{PianoRollWidth + (i * xDelta)}\" y1=\"0\" y2=\"{height}\" stroke-width=\"1\" stroke=\"#dddddd\" />";
            }
            foreach (var note in clip.Notes)
            {
                if (note.Pitch >= startNote && note.Pitch <= startNote + (octaves * 12))
                {
                    output += $"<rect style=\"fill:#ebebbc;fill-opacity:1;stroke:#8e8e8e;stroke-width:0.52916664;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1\" x=\"{PianoRollWidth + (note.Start * xDelta)}\" y=\"{(startNote + (octaves * 12) - note.Pitch) * yDelta}\" width=\"{note.Duration * xDelta}\" height=\"{yDelta}\" />";
                }
            }
            output += "</svg>";
            return output;
        }
    }
}
