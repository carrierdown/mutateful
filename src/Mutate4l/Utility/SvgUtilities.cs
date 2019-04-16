using Mutate4l.Core;

namespace Mutate4l.Utility
{
    public static class SvgUtilities
    {
        private static int PianoRollWidth { get; } = 30;

        public static string SvgFromClip(Clip clip, int x, int y, int width, int height, int numNotes, int startNote)
        {
            var output = $"<svg version=\"1.1\" baseProfile=\"full\" width=\"{width}\" height=\"{height}\" xmlns=\"http://www.w3.org/2000/svg\">";
            var yDelta = (decimal)height / numNotes;
            // piano + horizontal guides
            for (var i = 0; i <= numNotes; i++)
            {
                var colour = (i % 12 == 0 || i % 12 == 2 || i % 12 == 4 || i % 12 == 5 || i % 12 == 7 || i % 12 == 9 || i % 12 == 11) == true ? "ffffff" : "000000";
                output += $"<rect style=\"fill:#{colour};stroke:#8e8e8e;stroke-width:1;stroke-miterlimit:4\" x=\"{x}\" y=\"{y + 300 - yDelta - (i * yDelta)}\" width=\"30\" height=\"{yDelta}\" />";
                output += $"<line x1=\"{x + PianoRollWidth}\" x2=\"{x + width}\" y1=\"{y + height - yDelta - (i * yDelta)}\" y2=\"{y + height - yDelta - (i * yDelta)}\" stroke-width=\"1\" stroke=\"#bbbbbb\" />";
            }
            // vertical guides
            var xDelta = (width - PianoRollWidth) / clip.Length;
            for (decimal i = 0; i < clip.Length; i += 4m / 8) // 8ths for now
            {
                output += $"<line x1=\"{x + PianoRollWidth + (i * xDelta)}\" x2=\"{x + PianoRollWidth + (i * xDelta)}\" y1=\"{y}\" y2=\"{y + height}\" stroke-width=\"1\" stroke=\"#dddddd\" />";
            }
            foreach (var note in clip.Notes)
            {
                if (note.Pitch >= startNote && note.Pitch <= startNote + numNotes)
                {
                    output += $"<rect style=\"fill:#ebebbc;stroke:#8e8e8e;stroke-width:0.5;stroke-miterlimit:4\" x=\"{x + PianoRollWidth + (note.Start * xDelta)}\" y=\"{y + (startNote + numNotes - note.Pitch) * yDelta}\" width=\"{note.Duration * xDelta}\" height=\"{yDelta}\" />";
                }
            }
            output += "</svg>";
            return output;
        }
    }
}
