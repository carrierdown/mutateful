using System.Collections.Generic;

namespace Mutate4l.Core
{
    public class ChainedCommand
    {
        public List<Command> Commands { get; }
        public Clip[] SourceClips { get; }
        public ClipMetaData TargetMetaData { get; }

        public ChainedCommand(List<Command> commands, Clip[] sourceClips, ClipMetaData targetMetadata)
        {
            Commands = commands;
            SourceClips = sourceClips;
            TargetMetaData = targetMetadata;
        }

        public ChainedCommand() { }
    }
}
