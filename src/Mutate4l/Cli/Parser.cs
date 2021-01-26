using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mutate4l.Core;
using Mutate4l.State;

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

        public static ProcessResult<Formula> ParseFormula(string formula)
        {
            var lexer = new Lexer(formula);
            var (success, tokens, errorMessage) = lexer.GetTokens();
            if (!success) return new ProcessResult<Formula>(errorMessage);

            TreeToken syntaxTree;
            (success, syntaxTree, errorMessage) = CreateSyntaxTree(tokens);
            if (!success) return new ProcessResult<Formula>(errorMessage);

            // remaining steps:
            // convert any nested statements to inline clips - might involve some refactoring where parsing to commands and applying these can be done selectively and not as part of the fixed pipeline we have now

            Token[] commandTokens;
            (success, commandTokens, errorMessage) = ResolveAndFlattenSyntaxTree(syntaxTree);
            if (!success) return new ProcessResult<Formula>(errorMessage);

            var allReferencedClips = commandTokens.Where(x => x.IsClipReference)
                .Select(x => ClipReference.FromString(x.Value)).ToList();
            var sourceClipReferences = commandTokens.TakeWhile(x => x.IsClipReference).Select(x => ClipReference.FromString(x.Value)).ToList();
            var commandTokensLists = ExtractCommandTokensLists(commandTokens.Skip(sourceClipReferences.Count).ToArray());
            var commands = commandTokensLists.Select(ParseTokensToCommand).ToList();

            var parsedFormula = new Formula(commands, sourceClipReferences, allReferencedClips, new Dictionary<ClipReference, ClipSlot>());
            return new ProcessResult<Formula>(parsedFormula);
        }
        
        public static ProcessResult<ChainedCommand> ParseFormulaToChainedCommand(string formula, List<Clip> clips, ClipMetaData metadata)
        {
            var valid = new char[] { '[', ']' }.All(c => formula.IndexOf(c) >= 0);
            if (!valid) return new ProcessResult<ChainedCommand>($"Invalid formula: {formula}");

            var lexer = new Lexer(formula, clips);
            var (success, tokens, errorMessage) = lexer.GetTokens();
            if (!success) return new ProcessResult<ChainedCommand>(errorMessage);

            TreeToken syntaxTree;
            (success, syntaxTree, errorMessage) = CreateSyntaxTree(tokens);
            if (!success) return new ProcessResult<ChainedCommand>(errorMessage);

            Token[] commandTokens;
            (success, commandTokens, errorMessage) = ResolveAndFlattenSyntaxTree(syntaxTree);
            if (!success) return new ProcessResult<ChainedCommand>(errorMessage);
            
            var sourceClips = commandTokens.TakeWhile(x => x.Type == TokenType.InlineClip).Select(x => x.Clip).ToArray();
            var tokensToProcess = commandTokens.Skip(sourceClips.Length).ToArray();

            var commandTokensLists = ExtractCommandTokensLists(tokensToProcess);
            var commands = commandTokensLists.Select(ParseTokensToCommand).ToList();

            var chainedCommand = new ChainedCommand(commands, sourceClips, metadata);
            return new ProcessResult<ChainedCommand>(chainedCommand);
        }

        private static List<List<Token>> ExtractCommandTokensLists(Token[] tokensToProcess)
        {
            var commandTokensLists = new List<List<Token>>();
            var activeCommandTokenList = new List<Token>();
            if (tokensToProcess.Length == 0)
            {
                // Empty command, assume concat
                tokensToProcess = new Token[] {new Token(TokenType.Concat, "concat", 0),};
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
                        activeCommandTokenList = new List<Token> {token};
                    }
                }
                else
                {
                    activeCommandTokenList.Add(token);
                }
            }

            commandTokensLists.Add(activeCommandTokenList); // add last command token list
            return commandTokensLists;
        }

        private static ProcessResultArray<Token> ResolveAndFlattenSyntaxTree(TreeToken syntaxTree)
        {
            var flattenedTokens = new List<Token>();
            while (!syntaxTree.Children.All(x => x.AllValuesFetched))
            {
                foreach (var treeToken in syntaxTree.Children)
                {
                    var res = treeToken.FlattenedValues;
                    if (res.Success) flattenedTokens.AddRange(res.Result);
                    else return res;
                }
            }
            return new ProcessResultArray<Token>(flattenedTokens.ToArray());
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

        public static ProcessResult<TreeToken> CreateSyntaxTree(Token[] tokens)
        {
            // todo: support for nested statements need to be added here as well
            // todo: Parameters for commands need to be nested under their respective commands, otherwise the unpacking logic won't work properly...
            // e.g. loop 4 rat 2 3|4|5x2 6 currently leads to the 4 bleeding into the ratchet parameter list
            var rootToken = new TreeToken(TokenType.Root, "", 0);
            var insertionPoint = rootToken;
            
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (token.IsCommand && insertionPoint.Type != TokenType.Root)
                {
                    insertionPoint = insertionPoint.Parent;
                }
                if (i > 0 && tokens[i - 1].IsCommand && !token.IsCommand)
                {
                    insertionPoint = insertionPoint.Children[^1];
                }
                if (i + 1 < tokens.Length && token.IsValue && tokens[i + 1].IsOperatorToken)
                {
                    if (insertionPoint.IsOperatorToken && 
                        insertionPoint.Type == tokens[i + 1].Type)
                    {
                        insertionPoint.Children.Add(new TreeToken(tokens[i]).SetParent(insertionPoint));
                        if (i + 1 < tokens.Length && tokens[i + 1].IsOperatorToken)
                        {
                            i++;
                        }
                    }
                    else
                    {
                        var treeToken = new TreeToken(tokens[i + 1]);
                        treeToken.Children.Add(new TreeToken(tokens[i]).SetParent(treeToken));
                        i++;
                        insertionPoint.Children.Add(treeToken.SetParent(insertionPoint));
                        insertionPoint = treeToken;
                    }
                }
                else
                {
                    insertionPoint.Children.Add(new TreeToken(token).SetParent(insertionPoint));
                }

                if (insertionPoint.Children.Count > 2 && insertionPoint.Type == TokenType.RepeatOperator)
                {
                    return new ProcessResult<TreeToken>(
                        $"Too many operands specified for operator {insertionPoint.Type} near {insertionPoint.Children[^1]?.Value ?? insertionPoint.Value}, col {insertionPoint.Children[^1]?.Position ?? insertionPoint.Position}");
                }

                // Once we have an operator behind us we can move the insertion point up to root level again. This will not affect nested operators as tokens[i - 1] won't give us an operator token in these cases.
                if (i > 0 && tokens[i - 1].IsOperatorToken)
                {
                    while (insertionPoint.IsOperatorToken)
                        insertionPoint = insertionPoint.Parent;
                }
            }

            return new ProcessResult<TreeToken>(rootToken);
        }
    }
}