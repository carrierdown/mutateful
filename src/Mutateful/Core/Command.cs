using System.Collections.Generic;
using Mutateful.Compiler;

namespace Mutateful.Core
{
    public class Command
    {
        public TokenType Id { get; set; }
        public Dictionary<TokenType, List<Token>> Options { get; set; } = new Dictionary<TokenType, List<Token>>();
        public List<Token> DefaultOptionValues { get; set; } = new List<Token>();
    }
}
