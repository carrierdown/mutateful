using Mutate4l.IO;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using Mutate4l.Core;

namespace Mutate4l.Cli
{
    internal static class CliHandler
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

                if (formula.Contains("svg"))
                {
                    Console.WriteLine("Dumping SVG for clips");
                    foreach (var clip in clips)
                    {
                        DoSvg(formula, clips);
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
                var (channelNo, clipNo) = Parser.ResolveClipReference(arg);
                var clip = UdpConnector.GetClip(channelNo, clipNo);
                Console.WriteLine($"Clip length {clip.Length}");
                Console.WriteLine(Utility.IOUtilities.ClipToString(clip));
            }
        }

        private static void DoSvg(string command, IEnumerable<Clip> clips)
        {
            var arguments = command.Split(' ').Skip(1);
            var options = arguments.Where(x => x.StartsWith("-"));
            int numNotes = 24; // 2 octaves default
            int startNote = 60; // C3
            foreach (var option in options)
            {
                if (option.StartsWith("-numnotes:"))
                {
                    numNotes = int.Parse(option.Substring(option.IndexOf(':') + 1));
                }
                if (option.StartsWith("-startnote:"))
                {
                    startNote = int.Parse(option.Substring(option.IndexOf(':') + 1));
                }
                //Console.WriteLine($"option: {option}");
            }
            Console.WriteLine($"start: {startNote}");
            Console.WriteLine($"number of notes: {numNotes}");
            foreach (var clip in clips)
            {
                Console.WriteLine(SvgUtilities.SvgFromClip(clip, 0, 0, 370, 300, numNotes, startNote));
            }
        }
    }
}
