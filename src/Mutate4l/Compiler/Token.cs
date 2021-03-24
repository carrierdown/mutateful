using Mutate4l.Core;
using static Mutate4l.Compiler.TokenType;

namespace Mutate4l.Compiler
{
    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; }
        public int Position { get; }
        public Clip Clip { get; set; }

        public Token(TokenType type, string value, int position)
        {
            Clip = Clip.Empty;
            Type = type;
            Value = value;
            Position = position;
        }

        public Token(Token token)
        {
            Clip = token.Clip;
            Type = token.Type;
            Value = token.Value;
            Position = token.Position;
        }

        public Token(TreeToken treeToken)
        {
            Clip = treeToken.Clip;
            Type = treeToken.Type;
            Value = treeToken.Value;
            Position = treeToken.Position;
        }
        
        public Token(TokenType type, string value, Clip clip, int position) : this(type, value, position)
        {
            Clip = clip;
        }

        public bool IsClipReference => Type == TokenType.ClipReference;
        public bool IsOption => Type > _OptionsBegin && Type < _OptionsEnd && Value.StartsWith('-');
        public bool IsCommand => Type > _CommandsBegin && Type < _CommandsEnd && !Value.StartsWith('-');
        public bool IsOperatorToken => Type > _OperatorsBegin && Type < _OperatorsEnd;
        public bool IsOptionValue => IsEnumValue || IsValue;
        public bool IsEnumValue => Type > _EnumValuesBegin && Type < _EnumValuesEnd;
        public bool IsValue => Type > _ValuesBegin && Type < _ValuesEnd;
        public bool IsPureValue => Type > _ValuesBegin && Type < _PureValuesEnd;
    }
}
