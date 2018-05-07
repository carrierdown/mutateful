using Mutate4l.Cli;
using Mutate4l.Commands;
using Mutate4l.Dto;
using Mutate4l.IO;
using Mutate4l.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutate4l
{
    public static class ClipProcessor
    {
        public static ProcessResultArray<Clip> ProcessChainedCommand(ChainedCommand chainedCommand)
        {
            List<Clip> sourceClips = new List<Clip>();
            List<Clip> targetClips = new List<Clip>();

            // call out to OSC-layer to fetch actual data needed to process the command
            foreach (Tuple<int, int> clipReference in chainedCommand.SourceClips)
            {
                var clip = UdpConnector.GetClip(clipReference.Item1, clipReference.Item2);
                if (clip == null)
                {
                    return new ProcessResultArray<Clip>("Source clip was empty");
                }
                sourceClips.Add(clip);
            }

            sourceClips = sourceClips.Where(c => c.Notes.Count > 0).ToList();
            if (sourceClips.Count < 1)
            {
                throw new Exception("No source clips specified, or only empty clip(s) specified.");
            }

            Clip[] currentSourceClips = sourceClips.ToArray();
            ProcessResultArray<Clip> resultContainer = new ProcessResultArray<Clip>("No commands specified");
            foreach (var command in chainedCommand.Commands)
            {
                resultContainer = ProcessCommand(command, currentSourceClips);
                if (resultContainer.Success)
                {
                    currentSourceClips = resultContainer.Result;
                } else
                {
                    return resultContainer;
                }
            }
            if (resultContainer.Success)
            {
                // destination clip: if destination is specified, replace dest with created clip. If destination is not specified, created clips are added in new scenes after last specified source clip.
                if (chainedCommand.TargetClips.Count > 0)
                {
                    UdpConnector.SetClips(chainedCommand.TargetClips[0].Item1, chainedCommand.TargetClips[0].Item2, resultContainer.Result);
                }
                else
                {
                    // seems to be a bug here. subsequent fetching of clip contents from clips set this way return no content.
                    var lastSourceClip = chainedCommand.SourceClips[chainedCommand.SourceClips.Count - 1];
                    UdpConnector.SetClips(lastSourceClip.Item1, lastSourceClip.Item2, resultContainer.Result);
                }
            }
            return resultContainer;
        }

        public static ProcessResultArray<Clip> ProcessCommand(Command command, Clip[] clips)
        {
            ProcessResultArray<Clip> resultContainer;
            switch (command.Id)
            {
                case TokenType.Interleave:
                    resultContainer = Interleave.Apply(OptionParser.ParseOptions<InterleaveOptions>(command.Options), clips); 
                    break;
                case TokenType.Constrain:
                    resultContainer = Constrain.Apply(OptionParser.ParseOptions<ConstrainOptions>(command.Options), clips);
                    break;
                case TokenType.Slice:
                    resultContainer = Slice.Apply(OptionParser.ParseOptions<SliceOptions>(command.Options), clips);
                    break;
                case TokenType.Arpeggiate:
                    resultContainer = Arpeggiate.Apply(OptionParser.ParseOptions<ArpeggiateOptions>(command.Options), clips);
                    break;
                case TokenType.Monophonize:
                    resultContainer = Monophonize.Apply(clips);
                    break;
                case TokenType.Ratchet:
                    resultContainer = Ratchet.Apply(OptionParser.ParseOptions<RatchetOptions>(command.Options), clips);
                    break;
                case TokenType.Scan:
                    resultContainer = Scan.Apply(OptionParser.ParseOptions<ScanOptions>(command.Options), clips);
                    break;
                case TokenType.Filter:
                    resultContainer = Filter.Apply(OptionParser.ParseOptions<FilterOptions>(command.Options), clips);
                    break;
                default:
                    // todo: error here
                    return new ProcessResultArray<Clip>($"Unsupported command {command.Id}");
            }
            return resultContainer;
        }
    }
}
