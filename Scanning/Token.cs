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
}
