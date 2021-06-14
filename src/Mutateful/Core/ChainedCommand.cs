using System.Collections.Generic;

namespace Mutateful.Core
{
    public class ChainedCommand
    {
        public List<Command> Commands { get; }
        public Clip[] SourceClips { get; }
        public ClipMetaData TargetMetaData { get; }

        public static readonly ChainedCommand Empty = new ChainedCommand();
        
        public ChainedCommand(List<Command> commands, Clip[] sourceClips, ClipMetaData targetMetadata)
        {
            Commands = commands;
            SourceClips = sourceClips;
            TargetMetaData = targetMetadata;
        }

        public ChainedCommand()
        {
            Commands = new List<Command>();
            SourceClips = new Clip[0];
            TargetMetaData = new ClipMetaData();
        }
    }
}
