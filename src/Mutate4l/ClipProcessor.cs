using Mutate4l.Cli;
using Mutate4l.ClipActions;
using Mutate4l.Dto;
using Mutate4l.IO;
using System;
using System.Collections.Generic;

namespace Mutate4l
{
    public class ClipProcessor
    {
	public ProcessResult ProcessCommand(Command command)
        {
            List<Clip> sourceClips = new List<Clip>();
            List<Clip> targetClips = new List<Clip>();

            // call out to OSC-layer to fetch actual data needed to process the command
            foreach (Tuple<int, int> clipReference in chainedCommand.SourceClips)
            {
                sourceClips.Add(UdpConnector.GetClip(clipReference.Item1, clipReference.Item2));
            }
            if (sourceClips.Count < 1)
            {
                throw new Exception("No source clips specified");
            }
            if (sourceClips.Count < 2)
            {
                sourceClips.Add(sourceClips[0]);
            }
            Clip[] currentSourceClips = sourceClips.ToArray();
            ProcessResult resultContainer = new ProcessResult("No commands specified");
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
                    var lastSourceClip = chainedCommand.SourceClips[chainedCommand.SourceClips.Count - 1];
                    UdpConnector.SetClips(lastSourceClip.Item1, lastSourceClip.Item2, resultContainer.Result);
                }
            }
            return resultContainer;
        }

        public ProcessResult ProcessCommand(Command command, Clip[] clips)
        {
            ProcessResult resultContainer;
            switch (command.Id)
            {
                case TokenType.Interleave:
                    resultContainer = Interleave.Apply(OptionParser.ParseOptions<InterleaveOptions>(command.Options), clips); 
                    break;
                case TokenType.Constrain:
                    resultContainer = Constrain.Apply(OptionParser.ParseOptions<ConstrainOptions>(command.Options), clips);
                    break;
                default:
                    // todo: error here
                    return new ProcessResult($"Unsupported command {command.Id}");
            }
            return resultContainer;
        }
    }
}
