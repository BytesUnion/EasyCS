using System.Text;

public enum TokenType
{
    Keyword, Identifier, Number, String, Operator, Delimiter, Comment, EOF
}

public class Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public int Line { get; }
    public int Column { get; }

    public Token(TokenType type, string value, int line, int column)
    {
        Type = type;
        Value = value;
        Line = line;
        Column = column;
    }

    public override string ToString() => $"Token({Type}, '{Value}', line:{Line}, col:{Column})";
}

public class Lexer
{
    private readonly string _input;
    private int _position;
    private int _line;
    private int _column;

    private static readonly HashSet<string> Keywords = new HashSet<string>
    {
        "print", "if", "elif", "else", "endif", "endf", "True", "False", "f", "return",
        "for", "to", "do", "endfor", "while", "endwhile", "in", "break",
        "class", "endclass", "init", "endinit", "extends", "super", "from", "use", "share", "load", "as",
        "null", "new", "prompt"
    };

    private static readonly HashSet<string> Operators = new HashSet<string>
    {
        "+", "-", "*", "/", "=", "==", "!=", ">", "<", ">=", "<=", "&&", "||",
        "**", "//", "%"
    };

    private static readonly HashSet<char> Delimiters = new HashSet<char>
    {
        '(', ')', '[', ']', '{', '}', ',', ':', '.'
    };

    public Lexer(string input)
    {
        _input = input;
        _position = 0;
        _line = 1;
        _column = 1;
    }

    public List<Token> TokenizeAll()
    {
        var tokens = new List<Token>();
        Token token;
        do
        {
            token = GetNextToken();
            tokens.Add(token);
        } while (token.Type != TokenType.EOF);
        return tokens;
    }

    public Token GetNextToken()
    {
        if (_position >= _input.Length)
        {
            return new Token(TokenType.EOF, "", _line, _column);
        }

        char currentChar = _input[_position];

        if (char.IsWhiteSpace(currentChar))
        {
            SkipWhitespace();
            return GetNextToken();
        }

        if (currentChar == '>' && _position + 1 < _input.Length && _input[_position + 1] == '>')
        {
            return LexComment();
        }

        if (currentChar == '"' || currentChar == '\'')
        {
            return LexString();
        }

        if (char.IsDigit(currentChar))
        {
            return LexNumber();
        }

        if (char.IsLetter(currentChar) || currentChar == '_')
        {
            return LexIdentifierOrKeyword();
        }

        if (IsOperatorChar(currentChar))
        {
            return LexOperator();
        }

        if (Delimiters.Contains(currentChar))
        {
            return LexDelimiter();
        }

        throw new Exception($"Unexpected character: {currentChar} at line {_line}, column {_column}");
    }

    private Token LexComment()
    {
        int startColumn = _column;
        StringBuilder sb = new StringBuilder();
        while (_position < _input.Length && _input[_position] != '\n')
        {
            sb.Append(_input[_position]);
            _position++;
            _column++;
        }
        return new Token(TokenType.Comment, sb.ToString(), _line, startColumn);
    }

    private Token LexString()
    {
        int startColumn = _column;
        char quote = _input[_position];
        StringBuilder sb = new StringBuilder();
        _position++;
        _column++;

        while (_position < _input.Length && _input[_position] != quote)
        {
            sb.Append(_input[_position]);
            _position++;
            _column++;
        }

        if (_position >= _input.Length)
        {
            throw new Exception($"Unterminated string at line {_line}, column {startColumn}");
        }

        _position++;
        _column++;

        return new Token(TokenType.String, sb.ToString(), _line, startColumn);
    }

    private Token LexNumber()
    {
        int startColumn = _column;
        StringBuilder sb = new StringBuilder();
        bool hasDecimalPoint = false;

        while (_position < _input.Length &&
               (char.IsDigit(_input[_position]) || (_input[_position] == '.' && !hasDecimalPoint)))
        {
            if (_input[_position] == '.')
            {
                hasDecimalPoint = true;
            }
            sb.Append(_input[_position]);
            _position++;
            _column++;
        }

        return new Token(TokenType.Number, sb.ToString(), _line, startColumn);
    }

    private Token LexIdentifierOrKeyword()
    {
        int startColumn = _column;
        StringBuilder sb = new StringBuilder();

        while (_position < _input.Length &&
               (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '_'))
        {
            sb.Append(_input[_position]);
            _position++;
            _column++;
        }

        string value = sb.ToString();
        TokenType type = Keywords.Contains(value) ? TokenType.Keyword : TokenType.Identifier;

        return new Token(type, value, _line, startColumn);
    }

    private Token LexOperator()
    {
        int startColumn = _column;
        StringBuilder sb = new StringBuilder();

        while (_position < _input.Length && IsOperatorChar(_input[_position]))
        {
            sb.Append(_input[_position]);
            _position++;
            _column++;

            if (Operators.Contains(sb.ToString()))
            {
                if (_position < _input.Length &&
                    Operators.Contains(sb.ToString() + _input[_position]))
                {
                    continue;
                }
                break;
            }
        }

        string value = sb.ToString();
        if (!Operators.Contains(value))
        {
            throw new Exception($"Invalid operator: {value} at line {_line}, column {startColumn}");
        }

        return new Token(TokenType.Operator, value, _line, startColumn);
    }

    private Token LexDelimiter()
    {
        int startColumn = _column;
        char delimiter = _input[_position];
        _position++;
        _column++;
        return new Token(TokenType.Delimiter, delimiter.ToString(), _line, startColumn);
    }

    private void SkipWhitespace()
    {
        while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
        {
            if (_input[_position] == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }
            _position++;
        }
    }

    private bool IsOperatorChar(char c)
    {
        return "+-*/=!><&|%".Contains(c);
    }
}