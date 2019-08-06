using System;
using System.Linq;
using Mutate4l.Core;

namespace Mutate4l.Utility
{
    public static class SvgUtilities
    {
        private const string NoteFill = "#55d400";
        private const string NoteStroke = "#233450";
        private const string PianoRollBackgroundMainFill = "#cbcbd3";
        private const string PianoRollBackgroundSharpFill = "#b3b3b9";
        private const string PianoRollVerticalGuideStroke = "#e6e6e6";
        private const string PianoRollHorizontalGuideStroke = "#bbbbbb";
        private const string PianoKeyWhiteFill = "#ffffff";
        private const string PianoKeyBlackFill = "#000000";
        private const string PianoKeyStroke = "#8e8e8e";
        
        private static int PianoRollWidth { get; } = 30;

        public static string SvgFromClip(Clip clip, int x, int y, int width, int height)
        {
            var highestNote = Math.Clamp(clip.Notes.Max(c => c.Pitch) + 4, 0, 127); // Leave 3 notes on each side
            var lowestNote = Math.Clamp(clip.Notes.Min(c => c.Pitch) - 3, 0, 127); // as padding
            var numNotes = highestNote - lowestNote + 1;

            var output = "<svg version=\"1.1\" baseProfile=\"full\" width=\"{width}\" height=\"{height}\" " +
                         "xmlns=\"http://www.w3.org/2000/svg\">" + Environment.NewLine;

            output += ClipToSvg(clip, x, y, width, height, numNotes, lowestNote, highestNote);
            output += "</svg>";
            return output;
        }

        public static string ClipToSvg(Clip clip, int x, int y, int width, int height, int numNotes, int lowestNote, int highestNote)
        {
            var output = $"<rect style=\"fill:{PianoRollBackgroundMainFill};stroke-width:0\" x=\"{x + PianoRollWidth}\" y=\"{y}\" " +
                         $"width=\"{width - PianoRollWidth}\" height=\"{height}\" />" + Environment.NewLine;

            var yDelta = (decimal) height / numNotes;
            // piano + horizontal guides
            var j = 0;
            for (var i = lowestNote; i <= highestNote; i++)
            {
                var pianoColour = (i % 12 == 0 || i % 12 == 2 || i % 12 == 4 || i % 12 == 5 || i % 12 == 7 || i % 12 == 9 || i % 12 == 11) == true
                    ? PianoKeyWhiteFill
                    : PianoKeyBlackFill;
                output += $"<rect style=\"fill:{pianoColour};stroke:{PianoKeyStroke};stroke-width:1;stroke-miterlimit:4\" " +
                          $"x=\"{x}\" y=\"{Math.Round(y + height - yDelta - (j * yDelta))}\" width=\"30\" " +
                          $"height=\"{Math.Round(yDelta)}\" />" + Environment.NewLine;
                if (i % 12 == 1 || i % 12 == 3 || i % 12 == 6 || i % 12 == 8 || i % 12 == 10)
                {
                    output += $"<rect style=\"fill:{PianoRollBackgroundSharpFill};stroke-width:0;\" x=\"{x + PianoRollWidth}\" " +
                              $"y=\"{Math.Round(y + height - yDelta - (j * yDelta))}\" width=\"{width - PianoRollWidth}\" " +
                              $"height=\"{Math.Round(yDelta)}\" />" + Environment.NewLine;
                }

                output += $"<line x1=\"{x + PianoRollWidth}\" x2=\"{x + width}\" y1=\"{Math.Round(y + height - yDelta - (j * yDelta))}\" " +
                          $"y2=\"{Math.Round(y + height - yDelta - (j * yDelta))}\" stroke-width=\"1\" " +
                          $"stroke=\"{PianoRollHorizontalGuideStroke}\" />" + Environment.NewLine;
                j++;
            }

            // vertical guides
            var xDelta = (width - PianoRollWidth) / clip.Length;
            for (decimal i = 0; i < clip.Length; i += .25m)
            {
                output +=
                    $"<line x1=\"{Math.Round(x + PianoRollWidth + (i * xDelta))}\" x2=\"{Math.Round(x + PianoRollWidth + (i * xDelta))}\" y1=\"{y}\" y2=\"{y + height}\" stroke-width=\"1\" stroke=\"{PianoRollVerticalGuideStroke}\" />" +
                    Environment.NewLine;
            }

            foreach (var note in clip.Notes)
            {
                if (note.Pitch >= lowestNote && note.Pitch <= highestNote)
                {
                    output +=
                        $"<rect style=\"fill:{NoteFill};stroke:{NoteStroke};stroke-width:0.5;stroke-miterlimit:4\" x=\"{Math.Round(x + PianoRollWidth + (note.Start * xDelta))}\" y=\"{Math.Round(y + (highestNote - note.Pitch) * yDelta)}\" width=\"{Math.Round(note.Duration * xDelta)}\" height=\"{Math.Round(yDelta)}\" />" +
                        Environment.NewLine;
                }
            }

            return output;
        }
    }
}
