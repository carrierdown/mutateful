using System.Collections.Generic;
using Mutate4l.Cli;

namespace Mutate4l.Core
{
    public class Command
    {
        public TokenType Id { get; set; }
        public Dictionary<TokenType, List<Token>> Options { get; set; } = new Dictionary<TokenType, List<Token>>();
        public List<Token> DefaultOptionValues { get; set; } = new List<Token>();
    }
}
