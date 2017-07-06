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

        public void ProcessCommand(Command command)
        {
            List<Clip> sourceClips = new List<Clip>();
            List<Clip> targetClips = new List<Clip>();

            // call out to OSC-layer to fetch actual data needed to process the command
            foreach (Tuple<int, int> clipReference in command.SourceClips)
            {
                sourceClips.Add(UdpConnector.GetClip(clipReference.Item1, clipReference.Item2));
            }
            // find correct Action class to call and pass in options and data
            IClipAction clipAction;
            switch (command.Id)
            {
                case TokenType.Interleave:
                    clipAction = new Interleave();
                    break;
                case TokenType.Constrain:
                    clipAction = new Constrain(command.Options);
                    break;
                default:
                    // todo: error here
                    return;
            }
            // todo: catch InvalidOptionException
            //clipAction.Initialize(command.Options);
            // call out to OSC-layer with resulting data
        }
    }
}
