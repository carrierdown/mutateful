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
        private static UdpConnector udpConnector;
        public static UdpConnector UdpConnector {
            get
            {
                if (udpConnector == null)
                {
                    udpConnector = new UdpConnector();
                    udpConnector.Open();
                }
                return udpConnector;
            }
        }

        // todo: Add support for chaining? Would need a syntax change to make sense, e.g. a1 explode pitch interleave mode time repeats 1 2 counts 1/4 1/8 1/16 => a2
        public ProcessResult ProcessCommand(Command command)
        {
            List<Clip> sourceClips = new List<Clip>();
            List<Clip> targetClips = new List<Clip>();

            // call out to OSC-layer to fetch actual data needed to process the command
            foreach (Tuple<int, int> clipReference in command.SourceClips)
            {
                sourceClips.Add(UdpConnector.GetClip(clipReference.Item1, clipReference.Item2));
            }
            if (sourceClips.Count < 2)
            {
                throw new Exception("Less than two source clips specified");
            }
            ProcessResult resultContainer;
            switch (command.Id)
            {
                case TokenType.Interleave:
                    resultContainer = Interleave.Apply(OptionParser.ParseOptions<InterleaveOptions>(command.Options), sourceClips.ToArray()); 
                    break;
                case TokenType.Constrain:
                    resultContainer = Constrain.Apply(OptionParser.ParseOptions<ConstrainOptions>(command.Options), sourceClips.ToArray());
                    break;
                default:
                    // todo: error here
                    return new ProcessResult($"Unsupported command {command.Id}");
            }
            if (!resultContainer.Success)
            {
                return resultContainer;
            }
            // destination clip: if destination is specified, replace dest with created clip. If destination is not specified, created clips are added in new scenes after last specified source clip.
            if (command.TargetClips.Count > 0)
            {
                UdpConnector.SetClips(command.TargetClips[0].Item1, command.TargetClips[0].Item2, resultContainer.Result);
            }
            else
            {
                var lastSourceClip = command.SourceClips[command.SourceClips.Count - 1];
                UdpConnector.SetClips(lastSourceClip.Item1, lastSourceClip.Item2, resultContainer.Result);
            }
            return resultContainer;
        }
    }
}
