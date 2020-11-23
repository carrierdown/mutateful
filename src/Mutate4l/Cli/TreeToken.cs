using System.Collections.Generic;
using System.Linq;
using Mutate4l.Core;
using static Mutate4l.Cli.TokenType;

namespace Mutate4l.Cli
{
    public class TreeToken
    {
        public TreeToken Parent { get; set; }
        public TokenType Type { get; }
        public string Value { get; }
        public int Position { get; }
        public Clip Clip { get; }

        public bool AllValuesFetched => CurrentIndex >= Children.Count && Children.All(x => x.AllValuesFetched);

        public ProcessResultArray<Token> NextValue
        {
            get {
                if (Type == Root)
                {
                    if (Children.Count == 0) return new ProcessResultArray<Token>("Error: Nothing to parse");
                    return Children[CurrentIndex++ % Children.Count].NextValue;
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
                if (IsPureValue)
                {
                    CurrentIndex++;
                    return new ProcessResultArray<Token>(new [] { new Token(this) });
                }
                CurrentIndex++;
                if (CurrentIndex == 1) return new ProcessResultArray<Token>(new [] { new Token(this) });
                return new ProcessResultArray<Token>(new Token[0]);
            }
        }

        public readonly List<TreeToken> Children = new List<TreeToken>();
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
        
        private ProcessResultArray<Token> ResolveRepeat()
        {
            var result = new List<Token>();
            if (!Children[0].IsPureValue || Children[1].Type != Number)
            {
                return new ProcessResultArray<Token>($"Unable to resolve REPEAT expression with values {Children[0].Value} and {Children[1].Value}");
            }
            var valueToRepeat = Children[0];
            if (int.TryParse(Children[1].Value, out var repeatCount))
            {
                for (var index = 0; index < repeatCount; index++)
                {
                    var res = valueToRepeat.NextValue;
                    if (res.Success) result.AddRange(res.Result);
                    else return res;
                }
                CurrentIndex = Children.Count; // mark this node as completed
            }
            return new ProcessResultArray<Token>(result.ToArray());
        }

        private ProcessResultArray<Token> ResolveAlternate()
        {
            if (Children.Count < 2) return new ProcessResultArray<Token>("Unable to resolve ALTERNATE expression: At least two values are needed.");
            return Children[CurrentIndex++ % Children.Count].NextValue;
        }

        private ProcessResultArray<Token> ResolveUnsupportedOperator()
        {
            return new ProcessResultArray<Token>($"Unsupported operator (value is {Value})");
        }
    }
}
