namespace Mutateful.Compiler;

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

        (success, errorMessage) = AreTokensValid(tokens);
        if (!success)
            return new ProcessResult<Formula>($"Parsing of formula ${formula} was aborted: {errorMessage}");
        TreeToken syntaxTree;
        (success, syntaxTree, errorMessage) = CreateSyntaxTree(tokens);
        if (!success) return new ProcessResult<Formula>(errorMessage);

        // remaining steps:
        // convert any nested statements to inline clips - might involve some refactoring where parsing to commands and applying these can be done selectively and not as part of the fixed pipeline we have now

        Token[] commandTokens;
        (success, commandTokens, errorMessage) = ResolveAndFlattenSyntaxTree(syntaxTree);
        if (!success) return new ProcessResult<Formula>(errorMessage);

        try
        {
            var allReferencedClips = commandTokens.Where(x => x.IsClipReference)
                .Select(x => ClipReference.FromString(x.Value)).ToList();
            
            var sourceClipReferences = commandTokens.TakeWhile(x => x.IsClipReference).Select(x => ClipReference.FromString(x.Value)).ToList();
            var commandTokensLists = ExtractCommandTokensLists(commandTokens.Skip(sourceClipReferences.Count).ToArray());
            var commands = commandTokensLists.Select(ParseTokensToCommand).ToList();

            var parsedFormula = new Formula(commands, sourceClipReferences, allReferencedClips, new Dictionary<ClipReference, ClipSlot>());
            return new ProcessResult<Formula>(parsedFormula);
        }
        catch (ArgumentException ae)
        {
            return new ProcessResult<Formula>($"ParseFormula threw exception: {ae.Message}");
        } 
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

    private static ProcessResult<Token[]> ResolveAndFlattenSyntaxTree(TreeToken syntaxTree)
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
        return new ProcessResult<Token[]>(flattenedTokens.ToArray());
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

    public static (bool success, string error) AreTokensValid(Token[] tokens)
    {
        var parenLevel = 0;
        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            if (token.Type == TokenType.LeftParen)
            {
                parenLevel++;
            }
            else if (token.Type == TokenType.RightParen)
            {
                parenLevel--;
            }
            if (parenLevel < 0)
            {
                return (false, $"Encountered invalid parenthesis at position {token.Position}");
            }
            if (token.IsOperatorToken)
            {
                if (i > 0 && i + 1 < tokens.Length && !(tokens[i - 1].IsPureValue && !tokens[i + 1].IsPureValue))
                {
                    var ops = new List<string>();
                    if (!tokens[i - 1].IsPureValue) ops.Add(tokens[i - 1].Value);
                    if (!tokens[i + 1].IsPureValue) ops.Add(tokens[i + 1].Value);
                    var pluralizedValue = "value" + (ops.Count > 1 ? "s" : "");
                    return (false, $"Operator {token.Value} at position {token.Position} can not be used in conjunction with {pluralizedValue} {string.Join(" and ", ops)}");
                }
            }
        }
        if (parenLevel == 0) return (true, "");
        return (false, "Parentheses do not match up.");
    }
    
    public static ProcessResult<TreeToken> CreateSyntaxTree(Token[] tokens)
    {
        var rootToken = new TreeToken(TokenType.Root, "", 0);
        var insertionPoint = rootToken;
        
        for (var i = 0; i < tokens.Length; i++)
        {
            // if a left parenthesis is encountered, we create a new nested token and move the insertion point inside it
            if (tokens[i].Type == TokenType.LeftParen)
            {
                var treeToken = new TreeToken(TokenType.Nested, "()", tokens[i].Position).SetParent(insertionPoint);
                insertionPoint.Children.Add(treeToken);
                insertionPoint = treeToken;
                continue;
            }
            // if we encounter a right parenthesis...
            if (tokens[i].Type == TokenType.RightParen)
            {
                // ...we move up to the nearest left paren (or root as a sanity check) 
                while (insertionPoint.Type != TokenType.Nested && insertionPoint.Type != TokenType.Root)
                    insertionPoint = insertionPoint.Parent;
                // ...and then out of it
                insertionPoint = insertionPoint.Parent;
                continue;
            }
            if (tokens[i].IsCommand && insertionPoint.Type != TokenType.Root && insertionPoint.Type != TokenType.Nested)
            {
                insertionPoint = insertionPoint.Parent;
            }
            // if the token following a command is not another command, we assume command params and move insertion pointer accordingly
            if (i > 0 && tokens[i - 1].IsCommand && !tokens[i].IsCommand)
            {
                insertionPoint = insertionPoint.Children[^1];
            }
            // if we're on a value and an operator is coming up...
            if (i + 1 < tokens.Length && tokens[i].IsValue && tokens[i + 1].IsOperatorToken)
            {
                // check if we've already encountered an operator, and if the upcoming operator matches, continue adding values to it
                if (insertionPoint.IsOperatorToken && insertionPoint.Type == tokens[i + 1].Type)
                {
                    insertionPoint.Children.Add(new TreeToken(tokens[i]).SetParent(insertionPoint));
                    i++; // we discard the next operator since it's the same as we're already on
                }
                // otherwise, we add the new operator and set our current token as a child of it
                else
                {
                    var treeToken = new TreeToken(tokens[i + 1]);
                    treeToken.Children.Add(new TreeToken(tokens[i]).SetParent(treeToken));
                    i++; // discard next token since it's the operator we just added
                    insertionPoint.Children.Add(treeToken.SetParent(insertionPoint));
                    insertionPoint = treeToken;
                }
            }
            // no upcoming operator, so we simply add the token to the currently active insertion point
            else
            {
                insertionPoint.Children.Add(new TreeToken(tokens[i]).SetParent(insertionPoint));
            }

            if (insertionPoint.Children.Count > 2 && insertionPoint.Type == TokenType.RepeatOperator)
            {
                return new ProcessResult<TreeToken>(
                    $"Too many operands specified for operator {insertionPoint.Type} near {insertionPoint.Children[^1]?.Value ?? insertionPoint.Value}, col {insertionPoint.Children[^1]?.Position ?? insertionPoint.Position}");
            }

            // Once we have an operator behind us we can move the insertion point up to root level again. This will not affect nested operators as tokens[i - 1] won't give us an operator token in these cases
            // (recall that we discard the upcoming operator token when dealing with them above, thus i-1 will give us a value type in this situation).
            if (i > 0 && tokens[i - 1].IsOperatorToken)
            {
                while (insertionPoint.IsOperatorToken)
                    insertionPoint = insertionPoint.Parent;
            }
        }

        return new ProcessResult<TreeToken>(rootToken);
    }
}