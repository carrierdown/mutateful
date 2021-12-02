using static Mutateful.Compiler.TokenType;

namespace Mutateful.Compiler;

public class TreeToken
{
    public TreeToken Parent { get; set; }
    public TokenType Type { get; }
    public string Value { get; }
    public int Position { get; }
    public Clip Clip { get; }

    public bool AllValuesFetched => CurrentIndex > 0 && CurrentIndex >= Children.Count && Children.All(x => x.AllValuesFetched);

    public ProcessResult<Token[]> FlattenedValues
    {
        // todo: test how this behaves with invalid input like e.g. 60|62 61 shuffle 1 2
        get {
            if (IsCommand)
            {
                var tokens = new List<Token> { new Token(this) };
                return ResolveChildren(tokens);
            }
            if (IsOperatorToken)
            {
                return Type switch
                {
                    RepeatOperator => ResolveRepeat(),
                    AlternationOperator => ResolveAlternate(),
                    _ => ResolveUnsupportedOperator()
                };
            }
            
            CurrentIndex++;
            return new ProcessResult<Token[]>(new [] { new Token(this) });
        }
    }

    public readonly List<TreeToken> Children = new();
    private int CurrentIndex;

    public TreeToken() { }
    
    public TreeToken(TokenType type, string value, int position)
    {
        Clip = Clip.Empty;
        Type = type;
        Value = value;
        Position = position;
    }

    public TreeToken(Token token)
    {
        Clip = token.Clip;
        Type = token.Type;
        Value = token.Value;
        Position = token.Position;
    }

    public TreeToken SetParent(TreeToken parent)
    {
        Parent = parent;
        return this;
    }
    
    public bool HasChildren => Children.Count > 0;
    public bool IsClipReference => Type == TokenType.ClipReference;
    public bool IsOption => Type > _OptionsBegin && Type < _OptionsEnd && Value.StartsWith('-');
    public bool IsCommand => Type > _CommandsBegin && Type < _CommandsEnd && !Value.StartsWith('-');
    public bool IsOperatorToken => Type > _OperatorsBegin && Type < _OperatorsEnd;
    public bool IsOptionValue => IsEnumValue || IsValue;
    public bool IsEnumValue => Type > _EnumValuesBegin && Type < _EnumValuesEnd;
    public bool IsValue => Type > _ValuesBegin && Type < _ValuesEnd;
    public bool IsPureValue => Type > _ValuesBegin && Type < _PureValuesEnd;

    private ProcessResult<Token[]> ResolveChildren(List<Token> tokens)
    {
        if (Children.Count == 0)
        {
            CurrentIndex++;
            return new ProcessResult<Token[]>(tokens.ToArray());
        }
        while (!AllValuesFetched)
        {
            foreach (var treeToken in Children)
            {
                var res = treeToken.FlattenedValues;
                if (res.Success)
                {
                    tokens.AddRange(res.Result);
                    CurrentIndex++;
                }
                else return res;
            }
        }
        return new ProcessResult<Token[]>(tokens.ToArray());
    }

    private ProcessResult<Token[]> ResolveRepeat()
    {
        var result = new List<Token>();
        if (!Children[0].IsPureValue || Children[1].Type != Number)
        {
            return new ProcessResult<Token[]>($"Unable to resolve REPEAT expression with values {Children[0].Value} and {Children[1].Value}");
        }
        var valueToRepeat = Children[0];
        var repCountStatus = Children[1].FlattenedValues;
        if (!repCountStatus.Success) return repCountStatus;
        if (int.TryParse(repCountStatus.Result[0].Value, out var repeatCount))
        {
            for (var index = 0; index < repeatCount; index++)
            {
                var res = valueToRepeat.FlattenedValues;
                if (res.Success) result.AddRange(res.Result);
                else return res;
            }
            CurrentIndex = Children.Count; // mark this node as completed
        }
        return new ProcessResult<Token[]>(result.ToArray());
    }

    private ProcessResult<Token[]> ResolveAlternate()
    {
        if (Children.Count < 2) return new ProcessResult<Token[]>("Unable to resolve ALTERNATE expression: At least two values are needed.");
        return Children[CurrentIndex++ % Children.Count].FlattenedValues;
    }

    private ProcessResult<Token[]> ResolveUnsupportedOperator()
    {
        return new ProcessResult<Token[]>($"Unsupported operator (value is {Value})");
    }
}