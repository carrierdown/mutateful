using Mutate4l.Dto;
using Mutate4l.IO;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutate4l.Cli
{
    class CliHandler
    {
        public static void Start()
        {
            while (true)
            {
                var result = UdpConnector.WaitForData();
                if (UdpConnector.IsString(result))
                {
                    string text = UdpConnector.GetText(result);
                    Console.WriteLine(text);
                    continue;
                }
                (List<Clip> clips, string formula, ushort id, byte trackNo) = UdpConnector.DecodeData(result);
                Console.WriteLine($"Received {clips.Count} clips and formula: {formula}");
                if (formula.Contains("dump"))
                {
                    Console.WriteLine($"Dumping data for clips:");
                    foreach (var clip in clips)
                    {
                        Console.WriteLine(IOUtilities.ClipToString(clip));
                    }
                    continue;
                }
                var structuredCommand = Parser.ParseFormulaToChainedCommand(formula, clips, new ClipMetaData(id, trackNo));
                if (!structuredCommand.Success)
                {
                    Console.WriteLine(structuredCommand.ErrorMessage);
                    continue;
                }
                var status = ClipProcessor.ProcessChainedCommand(structuredCommand.Result);
                if (!status.Success)
                {
                    Console.WriteLine(status.ErrorMessage);
                }
            }
        }

        public static void DoDump(string command)
        {
            var arguments = command.Split(' ').Skip(1);
            foreach (var arg in arguments)
            {
                var clipReference = Parser.ResolveClipReference(arg);
                var clip = UdpConnector.GetClip(clipReference.Item1, clipReference.Item2);
                Console.WriteLine($"Clip length {clip.Length}");
                Console.WriteLine(Utility.IOUtilities.ClipToString(clip));
                /*                                foreach (var note in clip.Notes)
                                                {
                                                    Console.WriteLine($"Note start: {note.Start} duration: {note.Duration} pitch: {note.Pitch} velocity: {note.Velocity}");
                                                }*/
            }
        }

        public static void DoSvg(string command)
        {
            var arguments = command.Split(' ').Skip(1);
            var options = arguments.Where(x => x.StartsWith("-"));
            var clipReferences = arguments.Except(options);
            int octaves = 2;
            int startNote = 60; // C3
            foreach (var option in options)
            {
                if (option.StartsWith("-octaves:"))
                {
                    octaves = int.Parse(option.Substring(option.IndexOf(':') + 1));
                }
                if (option.StartsWith("-startnote:"))
                {
                    startNote = int.Parse(option.Substring(option.IndexOf(':') + 1));
                }
                //Console.WriteLine($"option: {option}");
            }
            Console.WriteLine($"start: {startNote}");
            Console.WriteLine($"octaves: {octaves}");
            foreach (var clipReference in clipReferences)
            {
                var clipRefParsed = Parser.ResolveClipReference(clipReference);
                var clip = UdpConnector.GetClip(clipRefParsed.Item1, clipRefParsed.Item2);
                Console.WriteLine(Utility.IOUtilities.ClipToString(clip));
                var output = "<svg version=\"1.1\" baseProfile=\"full\" width=\"400\" height=\"300\" xmlns=\"http://www.w3.org/2000/svg\">";
                var yDelta = 300 / (octaves * 12);
                // piano + horizontal guides
                for (int i = 0; i <= octaves * 12; i++)
                {
                    bool white = i % 12 == 0 || i % 12 == 2 || i % 12 == 4 || i % 12 == 5 || i % 12 == 7 || i % 12 == 9 || i % 12 == 11;
                    output += $"<rect style=\"fill:#{(white ? "ffffff" : "000000")};fill-opacity:1;stroke:#8e8e8e;stroke-width:1;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1\" x=\"0\" y=\"{300 - yDelta - (i * yDelta)}\" width=\"30\" height=\"{yDelta}\" />";
                    output += $"<line x1=\"30\" x2=\"400\" y1=\"{300 - yDelta - (i * yDelta)}\" y2=\"{300 - yDelta - (i * yDelta)}\" stroke-width=\"1\" stroke=\"#bbbbbb\" />";
                }
                // vertical guides
                var xDelta = 370 / clip.Length;
                for (decimal i = 0; i < clip.Length; i += 4m / 8) // 8ths for now
                {
                    output += $"<line x1=\"{30 + (i * xDelta)}\" x2=\"{30 + (i * xDelta)}\" y1=\"0\" y2=\"300\" stroke-width=\"1\" stroke=\"#dddddd\" />";
                }
                foreach (var note in clip.Notes)
                {
                    if (note.Pitch >= startNote && note.Pitch <= startNote + (octaves * 12))
                    {
                        output += $"<rect style=\"fill:#ebebbc;fill-opacity:1;stroke:#8e8e8e;stroke-width:0.52916664;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1\" x=\"{30 + (note.Start * xDelta)}\" y=\"{(startNote + (octaves * 12) - note.Pitch) * yDelta}\" width=\"{note.Duration * xDelta}\" height=\"{yDelta}\" />";
                    }
                }
                output += "</svg>";
                Console.WriteLine(output);
            }
        }
    }
}
