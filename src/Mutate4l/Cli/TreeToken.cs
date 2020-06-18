using System;
using System.Collections.Generic;
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

//        public bool AllValuesFetched => CurrentIndex >= Children.Length;

//        public ChildToken NextValue => HasChildren ? Children[CurrentIndex++ % Children.Length] : ValueAsChildToken;
        
        public List<TreeToken> Children = new List<TreeToken>();
//        private readonly ChildToken ValueAsChildToken;
        private int CurrentIndex;

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
    }
}
