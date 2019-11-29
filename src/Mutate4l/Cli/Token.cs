using System;
using Mutate4l.Core;
using static Mutate4l.Cli.TokenType;

namespace Mutate4l.Cli
{
    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Position { get; }
        public Clip Clip { get; }
        public OperatorType OperatorType { get; }

        public bool AllValuesFetched => CurrentIndex >= Children.Length;

        public ChildToken NextValue
        {
            get
            {
                if (OperatorType == OperatorType.Alternation) Console.WriteLine("Getting next value");
                if (HasChildren)
                {
                    if (OperatorType == OperatorType.Alternation) Console.WriteLine($"Has children, currentIndex {CurrentIndex}, allvaluesfetched {AllValuesFetched}");
                    var val = Children[CurrentIndex++ % Children.Length];
                    if (OperatorType == OperatorType.Alternation) Console.WriteLine($"Has children, currentIndex {CurrentIndex}, allvaluesfetched {AllValuesFetched}, val {val.Value}");
                    return val;
                }
                return ValueAsChildToken;
            }
        }
        
        private readonly ChildToken[] Children;
        private readonly ChildToken ValueAsChildToken;
        private int CurrentIndexField;
        private int CurrentIndex
        {
            get => CurrentIndexField;
            set
            {
                CurrentIndexField = value;
            }
        }

        public Token(TokenType type, string value, int position)
        {
            Children = new ChildToken[0];
            Clip = Clip.Empty;
            OperatorType = OperatorType.None;
            ValueAsChildToken = new ChildToken(type, value);

            Type = type;
            Value = value;
            Position = position;
        }

        public Token(Token token)
        {
            Children = token.Children;
            Clip = token.Clip;
            OperatorType = token.OperatorType;
            ValueAsChildToken = token.ValueAsChildToken;
            Type = token.Type;
            Value = token.Value;
            Position = token.Position;
        }
        
        public Token(TokenType type, string value, Clip clip, int position) : this(type, value, position)
        {
            Clip = clip;
        }

        public Token(TokenType type, int position, OperatorType operatorType, ChildToken[] children) : this(type, "", position)
        {
            OperatorType = operatorType;
            Children = children;
        }

        public bool HasChildren => Children.Length > 0;
        public bool IsClipReference => Type == TokenType.ClipReference;
        public bool IsOption => Type > _OptionsBegin && Type < _OptionsEnd;
        public bool IsCommand => Type > _CommandsBegin && Type < _CommandsEnd;
        public bool IsOperatorToken => Type > _OperatorsBegin && Type < _OperatorsEnd;
        public bool HasOperatorType => OperatorType != OperatorType.None;
        public bool IsOptionValue => IsEnumValue || IsValue;
        public bool IsEnumValue => Type > _EnumValuesBegin && Type < _EnumValuesEnd;
        public bool IsValue => Type > _ValuesBegin && Type < _ValuesEnd;
        public bool IsPureValue => Type > _ValuesBegin && Type < _PureValuesEnd;
    }
}
