using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mutateful.Core;

namespace Mutateful.Utility
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
        private const string FontFamily = "Helvetica, Consolas, Arial, Sans";
        private const int HeaderHeight = 20;
        private const int TextBaselineOffset = 4;
        private const int PianoRollWidth = 30;

        public static void GenerateSvgDoc(string formula, List<Clip> clips, Clip resultClip, int width, int height)
        {
            var contentHeight = height - HeaderHeight;
            var padding = 8;
            var resultClipWidth = (width - padding) / 2;
            var sourceClipWidth = resultClipWidth;
            var sourceClipHeight = contentHeight;

            if (clips.Count > 1)
            {
                sourceClipHeight -= padding;
                sourceClipHeight /= 2;
            }
            if (clips.Count > 2)
            {
                sourceClipWidth = resultClipWidth / 2;
            }

            var output = new StringBuilder($"<svg version=\"1.1\" baseProfile=\"full\" width=\"{width}\" height=\"{height}\" " +
                                           "xmlns=\"http://www.w3.org/2000/svg\">" + Environment.NewLine);

            output.Append($"<text x=\"{0}\" y=\"{HeaderHeight - TextBaselineOffset}\" fill=\"black\" font-family=\"{FontFamily}\" font-size=\"16\">Source clips</text>" +
                          $"<text x=\"{width - resultClipWidth}\" y=\"{HeaderHeight - TextBaselineOffset}\" fill=\"black\" font-family=\"{FontFamily}\" font-size=\"16\">Result</text>");
            
            var x = 0;
            var y = HeaderHeight;
            var highestNote = (clips.Max(c => c.Notes.Max(d => d.Pitch)) + 4) & 0x7F; // Leave 3 notes on each side
            var lowestNote = (clips.Min(c => c.Notes.Min(d => d.Pitch)) - 3) & 0x7F; // as padding, and clamp to 0-127 range
            var numNotes = highestNote - lowestNote + 1;
            
            for (var i = 0; i < Math.Min(4, clips.Count); i++)
            {
                var clip = clips[i];
                output.Append(ClipToSvg(clip, x, y, sourceClipWidth, sourceClipHeight, numNotes, lowestNote, highestNote));
                y += sourceClipHeight + padding;
                if (i == 1)
                {
                    x += sourceClipWidth + padding;
                    y = HeaderHeight;
                }
            }

            y = HeaderHeight;
            highestNote = (resultClip.Notes.Max(c => c.Pitch) + 4) & 0x7F; 
            lowestNote = (resultClip.Notes.Min(c => c.Pitch) - 3) & 0x7F; 
            numNotes = highestNote - lowestNote + 1;
            output.Append(ClipToSvg(resultClip, width - resultClipWidth, y, resultClipWidth, contentHeight, numNotes, lowestNote, highestNote));
            output.Append("</svg>");

            using (var file = File.AppendText($"Generated{DateTime.Now.Ticks}-clip.svg"))
            {
                file.Write(output.ToString());
            }
        }

        public static string ClipToSvg(Clip clip, int x, int y, int width, int height, int numNotes, int lowestNote, int highestNote)
        {
            var output = new StringBuilder(
                $"<rect style=\"fill:{PianoRollBackgroundMainFill};stroke-width:0\" x=\"{x + PianoRollWidth}\" y=\"{y}\" " +
                $"width=\"{width - PianoRollWidth}\" height=\"{height}\" />" + Environment.NewLine);

            var yDelta = (decimal) height / numNotes;
            // piano + horizontal guides
            var j = 0;
            for (var i = lowestNote; i <= highestNote; i++)
            {
                var pianoColour = (i % 12 == 0 || i % 12 == 2 || i % 12 == 4 || i % 12 == 5 || i % 12 == 7 || i % 12 == 9 || i % 12 == 11) == true
                    ? PianoKeyWhiteFill
                    : PianoKeyBlackFill;
                output.Append($"<rect style=\"fill:{pianoColour};stroke:{PianoKeyStroke};stroke-width:1;stroke-miterlimit:4\" " +
                          $"x=\"{x}\" y=\"{Math.Round(y + height - yDelta - (j * yDelta))}\" width=\"30\" " +
                          $"height=\"{Math.Round(yDelta)}\" />" + Environment.NewLine);
                if (i % 12 == 1 || i % 12 == 3 || i % 12 == 6 || i % 12 == 8 || i % 12 == 10)
                {
                    output.Append($"<rect style=\"fill:{PianoRollBackgroundSharpFill};stroke-width:0;\" x=\"{x + PianoRollWidth}\" " +
                              $"y=\"{Math.Round(y + height - yDelta - (j * yDelta))}\" width=\"{width - PianoRollWidth}\" " +
                              $"height=\"{Math.Round(yDelta)}\" />" + Environment.NewLine);
                }

                output.Append($"<line x1=\"{x + PianoRollWidth}\" x2=\"{x + width}\" y1=\"{Math.Round(y + height - yDelta - (j * yDelta))}\" " +
                          $"y2=\"{Math.Round(y + height - yDelta - (j * yDelta))}\" stroke-width=\"1\" " +
                          $"stroke=\"{PianoRollHorizontalGuideStroke}\" />" + Environment.NewLine);
                j++;
            }

            // vertical guides
            var xDelta = (width - PianoRollWidth) / clip.Length;
            for (decimal i = 0; i < clip.Length; i += .25m)
            {
                output.Append($"<line x1=\"{Math.Round(x + PianoRollWidth + (i * xDelta))}\" x2=\"{Math.Round(x + PianoRollWidth + (i * xDelta))}\" " +
                          $"y1=\"{y}\" y2=\"{y + height}\" stroke-width=\"1\" stroke=\"{PianoRollVerticalGuideStroke}\" />" + Environment.NewLine);
            }

            foreach (var note in clip.Notes)
            {
                if (note.Pitch >= lowestNote && note.Pitch <= highestNote)
                {
                    output.Append($"<rect style=\"fill:{NoteFill};stroke:{NoteStroke};stroke-width:0.5;stroke-miterlimit:4\" " +
                              $"x=\"{Math.Round(x + PianoRollWidth + (note.Start * xDelta))}\" y=\"{Math.Round(y + (highestNote - note.Pitch) * yDelta)}\" " +
                              $"width=\"{Math.Round(note.Duration * xDelta)}\" height=\"{Math.Round(yDelta)}\" />" + Environment.NewLine);
                }
            }

            if (!string.IsNullOrEmpty(clip.RawClipReference))
            {
                output.Append($"<text x=\"{x + width - Math.Round(width / 12f)}\" y=\"{y + height - Math.Round(height / 18f)}\" fill=\"black\" font-family=\"{FontFamily}\" font-size=\"16\">{clip.RawClipReference.ToUpper()}</text>");
            }

            return output.ToString();
        }
    }
}
