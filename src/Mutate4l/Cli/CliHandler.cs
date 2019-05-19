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
                bool dumpThisSession = false;
                var result = UdpConnector.WaitForData();
                if (UdpConnector.IsString(result))
                {
                    string text = UdpConnector.GetText(result);
                    Console.WriteLine(text);
                    continue;
                }
                (List<Clip> clips, string formula, ushort id, byte trackNo) = UdpConnector.DecodeData(result);
                Console.WriteLine($"Received {clips.Count} clips and formula: {formula}");
                if (formula.EndsWith(" dump"))
                {
                    Console.WriteLine("Dumping data for raw input");
                    Utilities.DumpByteArrayToConsole(result.Take(result.Length - 5).ToArray(), "input"); // chop off " dump" in raw data...
                    formula = formula.Substring(0, formula.Length - 5); // ...and formula
                    dumpThisSession = true;
                }

                if (formula.Contains("svg"))
                {
                    Console.WriteLine("Dumping SVG for clips");
                    DoSvg(formula, clips);
                    continue;
                }
                var chainedCommandWrapper = Parser.ParseFormulaToChainedCommand(formula, clips, new ClipMetaData(id, trackNo));
                if (!chainedCommandWrapper.Success)
                {
                    Console.WriteLine(chainedCommandWrapper.ErrorMessage);
                    continue;
                }
                var processedClipWrapper = ClipProcessor.ProcessChainedCommand(chainedCommandWrapper.Result);
                if (processedClipWrapper.Success && processedClipWrapper.Result.Length > 0)
                {
                    var processedClip = processedClipWrapper.Result[0];
                    byte[] clipData = IOUtilities.GetClipAsBytes(chainedCommandWrapper.Result.TargetMetaData.Id, processedClip).ToArray();
                    if (dumpThisSession)
                    {
                        Utilities.DumpByteArrayToConsole(clipData, "output");
                    }
                    UdpConnector.SetClipAsBytesById(clipData);
                }
                else
                    Console.WriteLine($"Error applying formula: {processedClipWrapper.ErrorMessage}");
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
