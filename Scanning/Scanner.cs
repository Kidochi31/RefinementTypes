using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static RefinementTypes.Scanning.TokenType;

namespace RefinementTypes.Scanning
{
    internal class Scanner
    {
        internal readonly string Source;
        internal readonly List<Token> Tokens = new List<Token>();

        int Start = 0;
        int Current = 0;

        int StartLine = 1;
        int StartColumn = 1;

        int Line = 1;
        int Column = 1;


        public Scanner(string source)
        {
            Source = source;
        }

        public List<Token> ScanTokens()
        {
            List<Token> tokens = new List<Token>();
            while (!IsAtEnd())
            {
                Token token = ScanNextToken();
                tokens.Add(token);
            }
            tokens.Add(ScanNextToken()); //EOF
            return tokens;
        }

        public Token ScanNextToken()
        {
            while (!IsAtEnd())
            {
                Start = Current;
                StartLine = Line;
                StartColumn = Column;

                Token? possibleToken = ScanToken();
                if (possibleToken is Token)
                {
                    return possibleToken;
                }
            }
            return new Token(EOF, "", Line, Column, Line, Column);
        }

        Token? ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': return CreateToken(LEFT_PAREN);
                case ')': return CreateToken(RIGHT_PAREN);
                case '{': return CreateToken(LEFT_BRACE);
                case '}': return CreateToken(RIGHT_BRACE);
                case ',': return CreateToken(COMMA);
                case ';': return CreateToken(SEMICOLON);

                case '[': return CreateToken(LEFT_SQUARE);
                case ']': return CreateToken(RIGHT_SQUARE);
                case ':': return CreateToken(COLON);
                case '#': return CreateToken(HASH);
                case '$': return CreateToken(DOLLAR);
                case '?': return CreateToken(QUESTION);
                case '~': return CreateToken(TILDE);

                case '.':
                    return CreateToken(Match('.') ? DOT_DOT : DOT);
                case '-':
                    return CreateToken(Match('=') ? MINUS_EQUALS :
                                      Match('>') ? MINUS_GT : MINUS);
                case '+':
                    return CreateToken(Match('=') ? PLUS_EQUALS : PLUS);
                case '*':
                    return CreateToken(Match('=') ? STAR_EQUALS : STAR);
                case '%':
                    return CreateToken(Match('=') ? PERCENT_EQUALS : PERCENT);
                case '^':
                    return CreateToken(Match('=') ? CARET_EQUALS : CARET);
                case '!':
                    if (Consume('=', "Expected '=' after '!'."))
                        return CreateToken(BANG_EQUALS);
                    return null;
                case '=':
                    return CreateToken(Match('>') ? EQUALS_GT : EQUALS);
                case '<':
                    return CreateToken(Match('=') ? LT_EQUALS :
                                      Match('<') ?
                                            Match('=') ? LT_LT_EQUALS : LT_LT :
                                      Match('-') ? LT_MINUS : LESS);
                case '>':
                    if (Peek() == '>' || Peek() == '=')
                        return CreateToken(GT_GLUE);
                    return CreateToken(GREATER);
                case '&':
                    return CreateToken(Match('=') ? AMPERSAND_EQUALS : AMPERSAND);
                case '|':
                    return CreateToken(Match('=') ? BAR_EQUALS : BAR);
                case '/':
                    if (Match('/'))
                    {
                        //a comment goes until the end of line
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                        return null;
                    }
                    else if (Match('*'))
                    {
                        //delimited comment goes until */
                        while (!IsAtEnd())
                        {
                            char current = Advance();
                            if (current == '*' && Match('/')) // */
                            {
                                break; //end commend
                            }
                        }
                        return null;
                    }
                    return CreateToken(Match('=') ? SLASH_EQUALS : SLASH);
                case ' ':
                case '\r':
                case '\t':
                case '\n':
                    //Ignore whitespace
                    return null;
                case '@':
                    if (Match('"')) return RawString();
                    return EscapedIdentifier();
                case '"':
                    return SimpleString();
                case '`':
                    return CustomLiteral();
                case '\'':
                    return CharLiteral();

                default:
                    if (IsDigit(c)) return Number();
                    else if (IsAlpha(c)) return SimpleIdentifier();

                    SentryCompiler.Error(Line, Column, $"Unexpected character '{c}'.");
                    return null;
            }
        }

        Token? CharLiteral()
        {
            bool terminated = false;
            int length = 0;
            char value = '\0';
            while (!IsAtEnd())
            {
                char current = Advance();
                if (current == '\n') //\n not allowed in simple strings
                    SentryCompiler.Error(Line, Column, "Char literal cannot contain new line.");
                if (current == '\'') //end on '
                {
                    terminated = true;
                    break;
                }
                length++;
                if (current == '\\')
                {
                    if (Match('\\')) { value = '\\'; continue; }
                    if (Match('\'')) { value = '\''; continue; }
                    if (Match('n')) { value = '\n'; continue; }
                    if (Match('0')) { value = '\0'; continue; }
                    if (Match('"')) { value = '"'; continue; }
                    if (Match('`')) { value = '`'; continue; }
                    SentryCompiler.Error(Line, Column, "Unescaped '\\'.");
                }
                value = current;
            }

            if (!terminated)
            {
                SentryCompiler.Error(Line, Column, "Unterminated char literal.");
                return null;
            }

            if (length != 1)
            {
                SentryCompiler.Error(Line, Column, "Char literal can only be one character.");
                return null;
            }

            return CreateCharToken(CHAR, value);
        }

        Token CustomLiteral()
        {
            bool terminated = false;
            StringBuilder builder = new StringBuilder();
            while (!IsAtEnd())
            {
                char current = Advance();
                if (current == '\n') //\n not allowed in custom literals
                    SentryCompiler.Error(Line, Column, "Custom literals cannot contain new line.");
                if (current == '`' && !(Peek() == '`')) //end on non-double `
                {
                    terminated = true;
                    break;
                }
                if (current == '`' && Match('`')) //`` becomes `
                {
                    builder.Append('`');
                    continue;
                }
                if (current == '\\')
                {
                    if (Match('\\')) { builder.Append('\\'); continue; }
                    if (Match('\'')) { builder.Append('\''); continue; }
                    if (Match('n')) { builder.Append('\n'); continue; }
                    if (Match('0')) { builder.Append('\0'); continue; }
                    if (Match('"')) { builder.Append('"'); continue; }
                    if (Match('`')) { builder.Append('`'); continue; }
                    SentryCompiler.Error(Line, Column, "Unescaped '\\'.");
                }
                builder.Append(current);
            }

            if (!terminated)
            {
                SentryCompiler.Error(Line, Column, "Unterminated custom literal.");
                return null;
            }

            int literalEndIndex = Current; //position of final `

            //ensure that there is a suffix
            if (!IsAlpha(Peek()))
            {
                SentryCompiler.Error(Line, Column, "Custom literal requires suffix");
                return null;
            }


            //find any literal suffix characters -> alphanumeric
            while (IsAlphaNumeric(Peek())) Advance();

            //Trim `...`
            string value = builder.ToString();
            string suffix = Source[(literalEndIndex + 1)..Current];
            return CreateCustomLiteralToken(CUSTOM, value, suffix);
        }

        Token EscapedIdentifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            //Trim @...
            string text = Source[(Start + 1)..Current];
            return CreateIdentifierToken(IDENTIFIER, text);
        }

        Token SimpleIdentifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            string text = Source[Start..Current];
            TokenType type = Keywords.ContainsKey(text) ? Keywords[text] : IDENTIFIER;

            if (type == TRUE) return CreateBoolToken(type, true);
            if (type == FALSE) return CreateBoolToken(type, false);

            return type == IDENTIFIER ? CreateIdentifierToken(type, text) : CreateToken(type);
        }

        Token Number()
        {
            //Find the rest of the digits, allowing _
            while (IsMiddleDigit(Peek())) Advance();

            //find decimal point, with a digit after it
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                //It is a floating point number
                //consume '.'
                Advance();

                while (IsMiddleDigit(Peek())) Advance();//find the rest of the digits

                int finalDigitIndex = Current;

                //Find any number suffix characters
                if (IsAlpha(Peek())) //Must start with letter
                {
                    while (IsAlphaNumeric(Peek())) Advance(); //find all alphanumeric
                }
                string numberText = Source[Start..finalDigitIndex];
                string suffixText = finalDigitIndex + 1 <= Current ? Source[(finalDigitIndex + 1)..Current] : "";
                return CreateFloatToken(DECIMAL_FLOAT, double.Parse(numberText), suffixText);
            }
            else //It is an integer number
            {
                int finalDigitIndex = Current;

                //Find any number suffix characters
                if (IsAlpha(Peek())) //Must start with letter
                {
                    while (IsAlphaNumeric(Peek())) Advance(); //find all alphanumeric
                }
                //string numberText = Source[Start..Current];
                string numberText = Source[Start..finalDigitIndex];
                string suffixText = finalDigitIndex + 1 <= Current ? Source[(finalDigitIndex + 1)..Current] : "";
                return CreateIntegerToken(DECIMAL_INTEGER, long.Parse(numberText), suffixText);
            }
        }

        Token? RawString()
        {
            bool terminated = false;
            StringBuilder builder = new StringBuilder();
            while (!IsAtEnd())
            {
                char current = Advance();
                if (current == '"' && !(Peek() == '"')) //end on non-double "
                {
                    terminated = true;
                    break;
                }
                if (current == '"' && Match('"')) //"" becomes "
                {
                    builder.Append('"');
                    continue;
                }
                builder.Append(current);
            }

            if (!terminated)
            {
                SentryCompiler.Error(Line, Column, "Unterminated string.");
                return null;
            }

            //Trim @"..."
            return CreateStringToken(STRING, builder.ToString());
        }

        Token? SimpleString()
        {
            bool terminated = false;
            StringBuilder builder = new StringBuilder();
            while (!IsAtEnd())
            {
                char current = Advance();
                if (current == '\n') //\n not allowed in simple strings
                    SentryCompiler.Error(Line, Column, "Simple string cannot contain new line.");
                if (current == '"' && !(Peek() == '"')) //end on non-double "
                {
                    terminated = true;
                    break;
                }
                if (current == '"' && Match('"')) //"" becomes "
                {
                    builder.Append('"');
                    continue;
                }
                if (current == '\\')
                {
                    if (Match('\\')) { builder.Append('\\'); continue; }
                    if (Match('\'')) { builder.Append('\''); continue; }
                    if (Match('n')) { builder.Append('\n'); continue; }
                    if (Match('0')) { builder.Append('\0'); continue; }
                    if (Match('"')) { builder.Append('"'); continue; }
                    if (Match('`')) { builder.Append('`'); continue; }
                    SentryCompiler.Error(Line, Column, "Unescaped '\\'.");
                }
                builder.Append(current);
            }

            if (!terminated)
            {
                SentryCompiler.Error(Line, Column, "Unterminated string.");
                return null;
            }

            //Trim "..."
            return CreateStringToken(STRING, builder.ToString());
        }

        char PeekNext()
        {
            if (Current + 1 >= Source.Length) return '\0';
            return Source[Current + 1];
        }

        //Lookahead at the current character
        char Peek()
        {
            if (IsAtEnd()) return '\0';
            return Source[Current];
        }

        //Will check the next char and conditionally eat it if expected
        bool Match(char expected)
        {
            if (Peek() != expected) return false;
            Advance();
            return true;
        }

        bool Consume(char expected, string message)
        {
            if (Match(expected))
            {
                return true;
            }

            SentryCompiler.Error(Line, Column, message);
            return false;
        }

        // returns current char and moves to next
        //Also increases line count if necessary
        char Advance()
        {
            char current = Source[Current++];
            Column++;
            if (current == '\n') { Line++; Column = 1; }
            return current;
        }

        IdentifierToken CreateIdentifierToken(TokenType type, string value)
        {
            string text = Source[Start..Current];
            IdentifierToken newToken = new IdentifierToken(type, text, StartLine, StartColumn, Line, Column, value);
            Tokens.Add(newToken);
            return newToken;
        }

        BoolToken CreateBoolToken(TokenType type, bool value)
        {
            string text = Source[Start..Current];
            BoolToken newToken = new BoolToken(type, text, StartLine, StartColumn, Line, Column, value);
            Tokens.Add(newToken);
            return newToken;
        }

        CustomLiteralToken CreateCustomLiteralToken(TokenType type, string value, string suffix)
        {
            string text = Source[Start..Current];
            CustomLiteralToken newToken = new CustomLiteralToken(type, text, StartLine, StartColumn, Line, Column, value, suffix);
            Tokens.Add(newToken);
            return newToken;
        }

        FloatToken CreateFloatToken(TokenType type, double value, string suffix)
        {
            string text = Source[Start..Current];
            FloatToken newToken = new FloatToken(type, text, StartLine, StartColumn, Line, Column, value, suffix);
            Tokens.Add(newToken);
            return newToken;
        }

        IntegerToken CreateIntegerToken(TokenType type, long value, string suffix)
        {
            string text = Source[Start..Current];
            IntegerToken newToken = new IntegerToken(type, text, StartLine, StartColumn, Line, Column, value, suffix);
            Tokens.Add(newToken);
            return newToken;
        }


        CharToken CreateCharToken(TokenType type, char value)
        {
            string text = Source[Start..Current];
            CharToken newToken = new CharToken(type, text, StartLine, StartColumn, Line, Column, value);
            Tokens.Add(newToken);
            return newToken;
        }

        StringToken CreateStringToken(TokenType type, string value)
        {
            string text = Source[Start..Current];
            StringToken newToken = new StringToken(type, text, StartLine, StartColumn, Line, Column, value);
            Tokens.Add(newToken);
            return newToken;
        }

        Token CreateToken(TokenType type)
        {
            string text = Source[Start..Current];
            Token newToken = new Token(type, text, StartLine, StartColumn, Line, Column);
            Tokens.Add(newToken);
            return newToken;
        }


        bool IsAtEnd() => Current >= Source.Length;

        bool IsDigit(char c) => char.IsDigit(c);
        bool IsMiddleDigit(char c) => char.IsDigit(c) || c == '_';
        bool IsAlpha(char c) => char.IsAsciiLetter(c) || c == '_';
        bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

        static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            {"and",  AND },
            {"_", UNDERSCORE },
            {"let", LET },
            {"using", USING },
            {"as", AS },
            {"in", IN },
            {"do", DO },
            {"unless", UNLESS},
            //{"class", CLASS},
            {"else", ELSE },
            {"then", THEN },
            {"false", FALSE },
            //{"for", FOR },
            {"fun", FUN },
            {"pro", PRO },
            {"extern", EXTERN },
            {"if", IF },
            //{"nil", NIL },
            {"or", OR },
            {"not", NOT },
            {"xor", XOR },
            //{"print", PRINT },
            //{"return", RETURN },
            //{"super", SUPER },
            //{"this", THIS },
            {"true", TRUE },
            //{"var", VAR },
            {"while", WHILE },
            {"until", UNTIL },

        };
    }
}
