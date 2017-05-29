using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l
{
    public class Command
    {
        public TokenType Id { get; set; }
        public Dictionary<TokenType, List<string>> Options { get; set; }
        public List<Tuple<int, int>> SourceClips { get; set; }
        public List<Tuple<int, int>> TargetClips { get; set; }
    }
}
