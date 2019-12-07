using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mutate4l.Core;

namespace Mutate4l.Cli
{
    public static class Parser
    {
        public static Tuple<int, int> ResolveClipReference(string reference)
        {
            if (reference == "*") return new Tuple<int, int>(-1, -1);
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

        public static ProcessResult<ChainedCommand> ParseFormulaToChainedCommand(string formula, List<Clip> clips, ClipMetaData metadata)
        {
            var valid = new char[] { '[', ']' }.All(c => formula.IndexOf(c) >= 0);
            if (!valid) return new ProcessResult<ChainedCommand>($"Invalid formula: {formula}");

            var lexer = new Lexer(formula, clips);
            var result = lexer.GetTokens();
            if (!result.Success) return new ProcessResult<ChainedCommand>(result.ErrorMessage);
            var resolvedTokens = ResolveOperators(result.Result);
            if (!resolvedTokens.Success) return new ProcessResult<ChainedCommand>(resolvedTokens.ErrorMessage);
            Token[] commandTokens = ApplyOperators(resolvedTokens.Result);
            var commandTokensLists = new List<List<Token>>();
            var activeCommandTokenList = new List<Token>();
            var sourceClips = commandTokens.TakeWhile(x => x.Type == TokenType.InlineClip).Select(x => x.Clip).ToArray();
            var tokensToProcess = commandTokens.Skip(sourceClips.Count()).ToArray();

            if (tokensToProcess.Length == 0) 
            {
                // Empty command, assume concat
                tokensToProcess = new Token[] {new Token(TokenType.Concat, "concat", 0), };
            }

            foreach (var token in tokensToProcess)
            {
                if (token.IsCommand)
                {
                    if (activeCommandTokenList.Count == 0)
                    {
                        activeCommandTokenList.Add(token);
                    }
                    else
                    {
                        commandTokensLists.Add(activeCommandTokenList);
                        activeCommandTokenList = new List<Token> { token };
                    }
                }
                else
                {
                    activeCommandTokenList.Add(token);
                }
            }
            commandTokensLists.Add(activeCommandTokenList); // add last command token list
            var commands = commandTokensLists.Select(ParseTokensToCommand).ToList();

            var chainedCommand = new ChainedCommand(commands, sourceClips, metadata);
            return new ProcessResult<ChainedCommand>(chainedCommand);
        }

        public static Token[] ApplyOperators(Token[] tokens)
        {
            var processedTokens = new List<Token>(tokens.Length);
            var i = 0;
            var valueBlockStartIx = -1;
            while (i < tokens.Length)
            {
                if (tokens[i].IsPureValue || tokens[i].IsOperatorToken)
                {
                    if (valueBlockStartIx < 0) valueBlockStartIx = i;
                }
                else
                {
                    valueBlockStartIx = -1;
                }

                // Operators are processed for each block of pure values and operators, since they can operate on the entire block of values
                if (valueBlockStartIx >= 0)
                {
                    var ix = 0;
                    var containsBlockLevelOperators = false;
                    while (valueBlockStartIx + ix < tokens.Length && (tokens[valueBlockStartIx + ix].IsPureValue || tokens[valueBlockStartIx + ix].IsOperatorToken))
                    {
                        if (IsBlockLevelOperator(tokens[valueBlockStartIx + ix].Type)) containsBlockLevelOperators = true;
                        ix++;
                    }
                    var valueBlockEndIx = valueBlockStartIx + ix;

                    if (containsBlockLevelOperators)
                    {
                        var tokensInBlock = new List<Token>();
                        for (var y = valueBlockStartIx; y < valueBlockEndIx; y++) tokensInBlock.Add(tokens[y]);
                        while (tokensInBlock.Any(x => x.AllValuesFetched == false))
                        {
                            foreach (var token in tokensInBlock)
                            {
                                var nextToken = token.NextValue;
                                processedTokens.Add(new Token(nextToken.Type, nextToken.Value, token.Position));
                            }
                        }
                        i = valueBlockEndIx;
                        continue;
                    }
                }
                processedTokens.Add(tokens[i]);
                i++;
            }
            return processedTokens.ToArray();
        }

        private static bool IsBlockLevelOperator(TokenType type)
        {
            return type switch
            {
                TokenType.AlternationOperator => true,
                TokenType.RangeOperator => true,
                _ => false
            };
        }

        // Processes single entity operators such as repeat, and pre-processes block level operators such as alternate
        public static ProcessResultArray<Token> ResolveOperators(Token[] tokens)
        {
            if (!tokens.Any(x => x.IsOperatorToken)) return new ProcessResultArray<Token>(tokens);

            var processedTokens = new List<Token>(tokens.Length);
            var i = 0;
            while (i < tokens.Length)
            {
                if (i + 1 < tokens.Length && tokens[i + 1].IsOperatorToken)
                {
                    switch (tokens[i + 1].Type)
                    {
                        case TokenType.RepeatOperator when i + 2 < tokens.Length && tokens[i].Type == TokenType.Number:
                            if (!tokens[i + 2].IsPureValue) return new ProcessResultArray<Token>($"Expected a pure value (number or musical fraction) following repeat operator, but found {tokens[i + 2].Value}");
                            var valueToRepeat = tokens[i].Value;
                            if (int.TryParse(tokens[i + 2].Value, out var repeatCount))
                            {
                                for (var index = 0; index < repeatCount; index++)
                                {
                                    processedTokens.Add(new Token(tokens[i].Type, valueToRepeat, tokens[i].Position));
                                }
                            }
                            else
                            {
                                return new ProcessResultArray<Token>($"Unable to parse repeat operator. Tokens: {tokens[i].Value}, {tokens[i + 1].Value}, {tokens[i + 2].Value}");
                            }
                            i += 3;
                            break;
                        case TokenType.AlternationOperator when i + 2 < tokens.Length:
                            if (!(tokens[i].IsPureValue && tokens[i + 2].IsPureValue)) return new ProcessResultArray<Token>($"Unable to parse alternation operator. Tokens: {tokens[i].Value}, {tokens[i + 1].Value}, {tokens[i + 2].Value}");
                            var ix = i + 3;
                            var valuesToAlternate = new List<ChildToken>();
                            // todo: extract to more general function for extracting values interspersed with operators
                            valuesToAlternate.Add(new ChildToken(tokens[i].Type, tokens[i].Value));         // [n]'n
                            valuesToAlternate.Add(new ChildToken(tokens[i + 2].Type, tokens[i + 2].Value)); // n'[n]
                            while (ix + 1 < tokens.Length && tokens[ix].Type == TokenType.AlternationOperator && tokens[ix + 1].IsPureValue)
                            {
                                valuesToAlternate.Add(new ChildToken(tokens[ix + 1].Type, tokens[ix + 1].Value));
                                ix += 2;
                            }
                            processedTokens.Add(new Token(TokenType.AlternationOperator, tokens[i].Position, OperatorType.Alternation, valuesToAlternate.ToArray()));
                            i = ix; 
                            break;
                        default:
                            return new ProcessResultArray<Token>($"Error resolving operator with tokens {tokens[i].Value}, {tokens[i + 1].Value}, {tokens[i + 2].Value}");
                    }
                }
                else
                {
                    processedTokens.Add(tokens[i]);
                    i++;
                }
            }
            return new ProcessResultArray<Token>(processedTokens.ToArray());
        }

        private static Command ParseTokensToCommand(IEnumerable<Token> tokens)
        {
            var command = new Command();
            
            List<Token> tokensAsList = tokens.ToList();
            command.Id = tokensAsList[0].Type;
            var i = 1;
            while (i < tokensAsList.Count)
            {
                if (tokensAsList[i].IsOption)
                {
                    var type = tokensAsList[i].Type;
                    var values = new List<Token>();
                    i++;
                    while (i < tokensAsList.Count && tokensAsList[i].IsOptionValue)
                    {
                        values.Add(tokensAsList[i++]);
                    }
                    if (!command.Options.ContainsKey(type))
                    {
                        command.Options.Add(type, values);
                    }
                }
                else 
                {
                    while (i < tokensAsList.Count && tokensAsList[i].IsOptionValue) {
                        // If we don't get an option header but just one or more values, assume these are values for the default option
                        command.DefaultOptionValues.Add(tokensAsList[i++]);
                    }
                }
            }
            return command;
        }
    }
}