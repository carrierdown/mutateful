using Mutate4l.Cli;
using System;
using System.Collections.Generic;

namespace Mutate4l.Dto
{
    public class Command
    {
        public TokenType Id { get; set; }
        public Dictionary<TokenType, List<Token>> Options { get; set; } = new Dictionary<TokenType, List<Token>>();
        public List<Token> DefaultOptionValues { get; set; } = new List<Token>();
        public List<Tuple<int, int>> SourceClips { get; set; } = new List<Tuple<int, int>>();
        public List<Tuple<int, int>> TargetClips { get; set; } = new List<Tuple<int, int>>();
    }
}
