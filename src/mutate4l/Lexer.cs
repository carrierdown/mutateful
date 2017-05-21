using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;

namespace Mutate4l
{
    public enum TokenType
    {
        Backslash,
        Plus,
        Minus,
        Multiply,
        Period,
        Colon,
        Percent,
        Pipe,
        Exclamation,
        Destination,
        None
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Position { get; set; }

        public Token(TokenType type, string value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }
    }

    public class Lexer
    {
        private int Position = 0;
        private string Buffer;
        private int BufferLength;

        private Dictionary<char, TokenType> SingleOperators = new Dictionary<char, TokenType>
        {
            { ':', TokenType.Colon }
        };

        private Dictionary<string, TokenType> DoubleOperators = new Dictionary<string, TokenType>
        {
            { "=>", TokenType.Destination }
        };


        public Lexer(string buffer)
        {
            Buffer = buffer;
        }
        /*
        public List<Token> GetTokens()
        {

        }*/

        private bool IsSingleOperator(int pos)
        {
            return SingleOperators.Any(o => o.Key == Buffer[pos]);
        }

        private bool IsDoubleOperator(int pos)
        {
            if (Buffer.Length > pos + 1)
            {
                string nextTwoChars = $"{Buffer[pos]}{Buffer[pos + 1]}";
                return DoubleOperators.Any(o => o.Key == nextTwoChars);
            }
            return false;
        }

        public IEnumerable<Token> GetToken()
        {
            int pos = 0;
            while (pos < Buffer.Length)
            {
                pos = SkipNonTokens(pos);
                if (pos >= Buffer.Length)
                {
                    yield return null;
                }

                if (IsSingleOperator(pos))
                {
                    char value = Buffer[pos];
                    yield return new Token(SingleOperators[value], value.ToString(), pos);
                }

                if (IsDoubleOperator(pos))
                {
                    string value = $"{Buffer[pos]}{Buffer[pos + 1]}";
                    yield return new Token(DoubleOperators[value], value, pos);
                }

                /*                // The char at this.pos is part of a real token. Figure out which.
                                var char = this.buffer.charAt(this.position);

                                // '/' is treated specially, because it starts a comment if followed by
                                // another '/'. If not followed by another '/', it's the DIVIDE
                                // operator.
                                var nextChar = this.buffer.charAt(this.position + 1);

                                if (char === '/' && nextChar === '/')
                                {
                                    return this.processComment();
                                }
                                if (char === '/')
                                {
                                    return { type: TokenType.DIVIDE, value: '/', pos: this.position++};
                                }
                                if (char === ';' && nextChar === ';')
                                {
                                    return { type: TokenType.DOUBLE_SEMI, value: ';;', pos: this.position += 2};
                                }
                                if (char === '.' && nextChar === '.')
                                {
                                    return { type: TokenType.DOUBLE_PERIOD, value: '..', pos: this.position += 2};
                                }
                                if (char === '_' && nextChar === '*')
                                {
                                    return { type: TokenType.FILL, value: '_*', pos: this.position += 2};
                                }*/
                pos++;
            }
        }

        private int SkipNonTokens(int position)
        {
            while (position < Buffer.Length && (Buffer[position] == ' ' || Buffer[position] == '\t' || Buffer[position] == '\r' || Buffer[position] == '\n'))
            {
                position++;
            }
            return position;
        }
        /*
        '\\': TokenType.BACKSLASH,
        // '+': TokenType.PLUS,
        //'-': TokenType.MINUS,
        '*': TokenType.MULTIPLY,
        '.': TokenType.PERIOD,
        ':': TokenType.COLON,
        '%': TokenType.PERCENT,
        '|': TokenType.PIPE,
        '!': TokenType.EXCLAMATION,
        '?': TokenType.QUESTION,
        '#': TokenType.POUND,
        '&': TokenType.AMPERSAND,
        ';': TokenType.SEMI,
        ',': TokenType.COMMA,
        '(': TokenType.LEFT_PAREN,
        ')': TokenType.RIGHT_PAREN,
        '<': TokenType.LEFT_ANGLE,
        '>': TokenType.RIGHT_ANGLE,
        '{': TokenType.LEFT_BRACE,
        '}': TokenType.RIGHT_BRACE,
        '[': TokenType.LEFT_BRACKET,
        ']': TokenType.RIGHT_BRACKET,
        '=': TokenType.EQUALS,
        '\'': TokenType.SINGLE_QUOTE*/
            // };



            // Initialize the Lexer's buffer. This resets the lexer's internal
            // state and subsequent tokens will be returned starting with the
            // beginning of the new buffer.
      /*      public SetBuffer(string buffer)
    {
        Position = 0;
        Buffer = buffer;
        BufferLength = buffer.length;
    }

    getTokenSet(inputBuffer: string): Array<IToken> {
        var token: IToken,
            tokenSet: Array<IToken> = [];

        this.setBuffer(inputBuffer);
        while (token = this.token()) {
        tokenSet.push(token);
    }
        return tokenSet;
    }*/

    // Get the next token from the current buffer. A token is an object with
    // the following properties:
    // - name: name of the pattern that this token matched (taken from rules).
    // - value: actual string value of the token.
    // - pos: offset in the current buffer where the token starts.
    //
    // If there are no more tokens in the buffer, returns null. In case of
    // an error throws Error.
    /*
    token(): IToken {
        let token;
        do {
            token = this.nextToken();
        } while (token !== null && token.type === TokenType.COMMENT);
        return token;
    }

    nextToken(): IToken {
        this.skipNonTokens();
        if (this.position >= this.bufferLength) {
    return null;
}

// The char at this.pos is part of a real token. Figure out which.
var char = this.buffer.charAt(this.position);

// '/' is treated specially, because it starts a comment if followed by
// another '/'. If not followed by another '/', it's the DIVIDE
// operator.
var nextChar = this.buffer.charAt(this.position + 1);

        if (char === '/' && nextChar === '/') {
    return this.processComment();
}
        if (char === '/') {
    return { type: TokenType.DIVIDE, value: '/', pos: this.position++};
}
        if (char === ';' && nextChar === ';') {
    return { type: TokenType.DOUBLE_SEMI, value: ';;', pos: this.position += 2};
}
        if (char === '.' && nextChar === '.') {
    return { type: TokenType.DOUBLE_PERIOD, value: '..', pos: this.position += 2};
}
        if (char === '_' && nextChar === '*') {
    return { type: TokenType.FILL, value: '_*', pos: this.position += 2};
}
// Look it up in the table of operators
var op = this.operatorTable [char];
        // special case: x can also be part of an identifier, but if alone it is the repeat token
        if (op !== undefined) {
    let token:IToken = {
    type: op, value: char, pos: this.position, isolatedLeft: (this.getChar(this.position - 1) === ' '),
                isolatedRight: (this.getChar(this.position + 1) === ' ')
            };
    this.position++;
    return token;
}
        // Not an operator - so it's the beginning of another token.
        if (Lexer.isNote(char, nextChar)) {
    return this.processNote();
}
        if (Lexer.isAlpha(char) && Lexer.isAlpha(nextChar)) { // note that this makes one-letter identifiers impossible unless specifically detected below
    if (char === '$')
    {
        return this.processVariableName();
    }
    return this.processIdentifier();
}
        if (Lexer.isAlpha(char)) {
    // check for one-letter alphabetic tokens (a-z, A-Z, _, $)
    if (char === 'x')
    {
        return { type: TokenType.REPEAT, value: char, pos: this.position++};
    }
    if (char === '_')
    {
        return { type: TokenType.UNDERSCORE, value: char, pos: this.position++};
    }
}
        if (Lexer.isDigit(char) || char === '-') {
    return this.processNumber();
}
        if (char === '"') {
    return this.processQuote();
}
        throw Error('Token error at ' + this.position + ' ' + this.buffer.charAt(this.position));
}

private getChar(pos): string {
        if (pos >= 0 && pos< this.buffer.length) {
            return this.buffer.charAt(pos);
        }
        return "";
    }

    static isNewline(c): boolean {
        return c === '\r' || c === '\n';
    }

    static isDigit(c): boolean {
        return c >= '0' && c <= '9';
    }

    static isAlpha(c): boolean {
        return (c >= 'a' && c <= 'z') ||
            (c >= 'A' && c <= 'Z') ||
            c === '_' || c === '$' || c === '+';
    }

    static isValidVariableCharacter(c): boolean {
        return (c >= 'a' && c <= 'z') ||
            (c >= 'A' && c <= 'Z') ||
            c === '_' || c === '$' ||
            (c >= '0' && c <= '9');
    }

    static isNote(c1, c2): boolean {
        return ((c1 >= 'a' && c1 <= 'g') ||
            (c1 >= 'A' && c1 <= 'G')) &&
            ((c2 >= '0' && c2 <= '9') || c2 === '#' || c2 === '-');
    }

/*    static isAlphaNumeric(c): boolean {
        return (c >= 'a' && c <= 'z') ||
            (c >= 'A' && c <= 'Z') ||
            (c >= '0' && c <= '9') ||
            c === '_' || c === '$';
    }*/
    /*
    static isNoteCharacter(c): boolean {
        return (c >= 'a' && c <= 'g') ||
            (c >= 'A' && c <= 'G') ||
            (c >= '0' && c <= '9') ||
            c === '#' || c === '-';
    }

    private processNote(): IToken {
        var endpos = this.position + 1;
        while (endpos< this.bufferLength &&
        Lexer.isNoteCharacter(this.buffer.charAt(endpos))) {
    endpos++;
}

var token = {
type: TokenType.NOTE,
            value: this.buffer.substring(this.position, endpos),
            pos: this.position
        };
        this.position = endpos;
        return token;
}

private processNumber(): IToken {
        var endpos = this.position + 1;
        while (endpos< this.bufferLength && (Lexer.isDigit(this.buffer.charAt(endpos))
               || (this.buffer.charAt(endpos) === '.') && (this.buffer.charAt(endpos + 1) !== '.') )) // avoid parsing shorthand ranges as numbers, e.g. 1..4
        {
    endpos++;
}

var token: IToken = {
type: TokenType.NUMBER,
            value: this.buffer.substring(this.position, endpos),
            pos: this.position
        };
token.value = parseFloat(token.value);
        if (isNaN(token.value)) {
    // invalid number, throw error. fix this when rewriting lexer
    throw new Error('WARN: Lexer detected invalid number ' + token.value);
}

        this.position = endpos;
        return token;
}

private processComment(): IToken {
        var endpos = this.position + 2;
// Skip until the end of the line
var c = this.buffer.charAt(this.position + 2);
        while (endpos< this.bufferLength && !Lexer.isNewline(this.buffer.charAt(endpos))) {
    endpos++;
}

var token: IToken = {
type: TokenType.COMMENT,
            value: this.buffer.substring(this.position, endpos),
            pos: this.position
        };
        this.position = endpos + 1;
        return token;
}

private processIdentifier(): IToken {
        var endpos = this.position + 1;
        while (endpos< this.bufferLength &&
        Lexer.isAlpha(this.buffer.charAt(endpos))) {
    endpos++;
}

var token = {
type: TokenType.IDENTIFIER,
            value: this.buffer.substring(this.position, endpos),
            pos: this.position
        };
        this.position = endpos;
        return token;
}

private processVariableName(): IToken {
        var endpos = this.position + 1;
        while (endpos< this.bufferLength &&
        Lexer.isValidVariableCharacter(this.buffer.charAt(endpos))) {
    endpos++;
}

var token = {
type: TokenType.VARIABLE_NAME,
            value: this.buffer.substring(this.position, endpos),
            pos: this.position
        };
        this.position = endpos;
        return token;
}

private processQuote(): IToken {
        // this.pos points at the opening quote. Find the ending quote.
        var end_index = this.buffer.indexOf('"', this.position + 1);

        if (end_index === -1) {
            throw Error('Unterminated quote at ' + this.position);
        } else {
            var token = {
                type: TokenType.QUOTE,
                value: this.buffer.substring(this.position, end_index + 1),
                pos: this.position
            };
            this.position = end_index + 1;
            return token;
        }
    }

    private skipNonTokens(): void {
        while (this.position< this.bufferLength) {
    var c = this.buffer.charAt(this.position);
    if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
    {
        this.position++;
    }
    else
    {
        break;
    }
}
}*/
    }
}
