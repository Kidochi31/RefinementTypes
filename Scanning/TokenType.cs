using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinementTypes.Scanning
{
    internal enum TokenType
    {
        //TEMP KEYWORDS
        //PRINT,

        //Keyword tokens
        AND, OR, NOT, XOR, TRUE, FALSE, IF, THEN, ELSE, LET,
        USING, AS, IN, DO, FUN, PRO, EXTERN, UNLESS, WHILE, UNTIL,

        // Single-character tokens.
        PLUS, MINUS, STAR, SLASH, PERCENT, CARET, AMPERSAND,
        BAR, LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
        COMMA, DOT, SEMICOLON, LESS, GREATER, EQUALS,
        COLON, HASH, DOLLAR, QUESTION, TILDE,
        LEFT_SQUARE, RIGHT_SQUARE, UNDERSCORE,

        //Special token to parse >> vs > >
        //This is used for >=, >>, >>= and all >
        GT_GLUE,

        //These are used only after parsing
        GLUED_GT_EQUALS, GLUED_GT_GT, GLUED_GT_GT_EQUALS,

        //Multiple-character tokens.
        LT_LT,
        LT_EQUALS, DOT_DOT, MINUS_GT,
        EQUALS_GT, LT_MINUS, BANG_EQUALS,
        PLUS_EQUALS, MINUS_EQUALS, STAR_EQUALS, SLASH_EQUALS,
        PERCENT_EQUALS, AMPERSAND_EQUALS, BAR_EQUALS, CARET_EQUALS,
        LT_LT_EQUALS,

        // Literals.
        IDENTIFIER,
        DECIMAL_INTEGER, DECIMAL_FLOAT,
        STRING,
        CHAR,
        CUSTOM,

        // Keywords.

        EOF
    }
}
