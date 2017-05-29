using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mutate4l
{
    public class Parser
    {
        public static Tuple<int, int> ResolveClipReference(string reference)
        {
            string channel = "", clip = "";
            int c = 0;
            channel = reference[c++].ToString();
            while (c < reference.Length && Lexer.IsNumeric(reference[c]))
            {
                clip += reference[c++];
            }

            int x = 0, y = 0;
            x = Encoding.ASCII.GetBytes(channel.ToLower())[0] - Encoding.ASCII.GetBytes("a")[0];
            y = int.Parse(clip) - 1; // indexes into Live are 0-based
            return new Tuple<int, int>(x, y);
        }

        public static Command ParseTokensToCommand(IEnumerable<Token> tokens)
        {
            var command = new Command();
            command.Id = tokens.First().Type;
            List<Token> tokensAsList = tokens.ToList();

            /*                if (token.Type > TokenType._CommandsBegin && token.Type < TokenType._CommandsEnd)
                            {
                                command.Id = token.Type;
                            }
            */
            return command;
        }
    }
}
