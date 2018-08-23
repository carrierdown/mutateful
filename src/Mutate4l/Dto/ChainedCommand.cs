using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Dto
{
    public class ChainedCommand
    {
        //public List<Tuple<int, int>> SourceClips { get; set; } = new List<Tuple<int, int>>();
        public List<Tuple<int, int>> TargetClips { get; set; } = new List<Tuple<int, int>>();
        public List<Command> Commands { get; set; } = new List<Command>();
        public string TargetId { get; set; }
        public Clip[] SourceClips { get; set; }
    }
}
