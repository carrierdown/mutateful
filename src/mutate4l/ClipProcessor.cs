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
            // find correct Action class to call and pass in options and data
            IClipAction clipAction;
            switch (command.Id)
            {
                case TokenType.Interleave:
                    clipAction = new Interleave(); 
                    break;
                case TokenType.Constrain:
                    clipAction = new Constrain(command.Options); // throw exception if invalid options, translate to error status
                    break;
                default:
                    // todo: error here
                    return new ProcessResult($"Unsupported command {command.Id}");
            }
            // todo: catch InvalidOptionException
            //clipAction.Initialize(command.Options);

            var resultContainer = clipAction.Apply(sourceClips[0], sourceClips[1]);
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
