using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinementTypes.Scanning
{
    internal class Token
    {
        internal readonly TokenType Type;
        internal readonly string Lexeme;
        internal readonly int StartLine;
        internal readonly int StartColumn;
        internal readonly int EndLine;
        internal readonly int EndColumn;

        public Token(TokenType type, string lexeme, int startLine, int startColumn, int endLine, int endColumn)
        {
            Type = type;
            Lexeme = lexeme;
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
        }

        public override string ToString()
        {
            return $"{Type} '{Lexeme}' of type {Type}, found at [{StartLine}, {StartColumn}]";
        }
    }

    internal class IdentifierToken(TokenType type, string lexeme, int startLine, int startColumn, int endLine, int endColumn, string name)
        : Token(type, lexeme, startLine, startColumn, endLine, endColumn)
    {
        public readonly string IdentifierName = name;
    }

    internal abstract class LiteralToken(TokenType type, string lexeme, int startLine, int startColumn, int endLine, int endColumn)
        : Token(type, lexeme, startLine, startColumn, endLine, endColumn)
    {
    }

    internal class CharToken(TokenType type, string lexeme, int startLine, int startColumn, int endLine, int endColumn, char value)
        : LiteralToken(type, lexeme, startLine, startColumn, endLine, endColumn)
    {
        internal readonly char Value = value;
    }

    internal class IntegerToken(TokenType type, string lexeme, int startLine, int startColumn, int endLine, int endColumn, long value, string suffix)
        : LiteralToken(type, lexeme, startLine, startColumn, endLine, endColumn)
    {
        internal readonly long Value = value;
    }

    internal class FloatToken(TokenType type, string lexeme, int startLine, int startColumn, int endLine, int endColumn, double value, string suffix)
        : LiteralToken(type, lexeme, startLine, startColumn, endLine, endColumn)
    {
        internal readonly double Value = value;
    }

    internal class BoolToken(TokenType type, string lexeme, int startLine, int startColumn, int endLine, int endColumn, bool value)
        : LiteralToken(type, lexeme, startLine, startColumn, endLine, endColumn)
    {
        internal readonly bool Value = value;
    }
}
