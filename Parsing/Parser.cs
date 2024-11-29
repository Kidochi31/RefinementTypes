using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using RefinementTypes.Scanning;
using static RefinementTypes.Scanning.TokenType;

namespace RefinementTypes.Parsing
{
    internal partial class Parser
    {
        public class ParseError : Exception { }

        readonly Queue<Token> Tokens = new Queue<Token>();
        Token Previous = null;
        readonly Scanner Scanner;

        public Parser(Scanner scanner, List<Token> tokens)
        {
            Scanner = scanner;
            foreach (Token token in tokens)
            {
                Tokens.Enqueue(token);
            }
        }

        public Parser(Scanner scanner)
        {
            Scanner = scanner;
        }

        public List<TopLevel> ParseTopLevels()
        {
            List<TopLevel> topLevels = new List<TopLevel>();
            while (!IsAtEnd())
            {
                try
                {
                    topLevels.Add(TopLevel());
                }
                catch (ParseError e) { RecoverFromError(); }
            }
            return topLevels;
        }

        //Start -> ( TypeDeclaration | TypeTest | TypeFit | '\n' ) * ;
        TopLevel TopLevel()
        {
            if (Match(TYPE)) return TypeDeclaration();
            if (Match(TEST)) return TypeTest();
            if (Match(FIT)) return TypeFit();
            if (Match(PRINT)) return Print();
            throw new ParseError();
        }

        // Print -> 'print' name '\n';
        Print Print()
        {
            Token previous = Previous;
            IdentifierToken name = (IdentifierToken)Consume(IDENTIFIER, "expected identifier after 'print'.");
            Token newLine = ConsumeAny("expected new line at end of declaration.", NEW_LINE, EOF);

            return new Print(previous, name, newLine);
        }

        // TypeFit -> 'fit' type 'in' type '\n' ; 
        TypeFit TypeFit()
        {
            Token fitToken = Previous;
            Type fromType = Type();
            Token inToken = Consume(IN, "expected in after fit.");
            Type toType = Type();
            Token newLine = ConsumeAny("expected new line at end of declaration.", NEW_LINE, EOF);

            return new TypeFit(fitToken, fromType, inToken, toType, newLine);
        }


        // TypeDeclaration -> 'type' IDENTIFIER (':' Type)? '\n' ;
        TypeDeclaration TypeDeclaration()
        {
            Token typeToken = Previous;
            IdentifierToken name = (IdentifierToken)Consume(IDENTIFIER, "expected identifier after 'type'.");
            Token? colonToken = null;
            Type? type = null;
            if (Match(COLON))
            {
                colonToken = Previous;
                type = Type();
            }

            Token newLine = ConsumeAny("expected new line at end of declaration.", NEW_LINE, EOF);
            return new TypeDeclaration(typeToken, name, colonToken, type, newLine);
        }

        TypeTest TypeTest()
        {
            Token testToken = Previous;
            Type type = Type();
            Token newLine = ConsumeAny("expected new line at end of declaration.", NEW_LINE, EOF);
            return new TypeTest(testToken, type, newLine);
        }

        //Will ignore tokens until it finds: EOF, extern, fun, pro
        void RecoverFromError()
        {
            Advance();
            while (!IsAtEnd())
            {
                Token t = Peek();
                switch (t.Type)
                {
                    case EOF:
                    case EXTERN:
                    case FUN:
                    case PRO:
                        return;
                }
                Advance();
            }
        }

        void EnsureTokens(int amount)
        {
            if (amount > Tokens.Count)
            {
                int difference = amount - Tokens.Count;
                for (int i = 0; i < difference; i++)
                {
                    Tokens.Enqueue(Scanner.ScanNextToken());
                }
            }
        }

        Token JoinTogether(Token first, Token second)
        {
            //first must be GT_GLUE or >>
            switch (first.Type)
            {
                case GT_GLUE:
                    if (second.Type == EQUALS)
                    {
                        return new Token(GLUED_GT_EQUALS, ">=",
                            first.StartLine, first.StartColumn,
                            second.EndLine, second.EndColumn);
                    }
                    if (second.Type == GT_GLUE || second.Type == GREATER)
                    {
                        return new Token(GLUED_GT_GT, ">>",
                            first.StartLine, first.StartColumn,
                            second.EndLine, second.EndColumn);
                    }
                    throw Error(second, "> must be followed by = or >.");
                case GLUED_GT_GT:
                    if (second.Type == EQUALS)
                    {
                        return new Token(GLUED_GT_GT_EQUALS, ">>=",
                            first.StartLine, first.StartColumn,
                            second.EndLine, second.EndColumn);
                    }
                    throw Error(second, ">> must be followed by =.");
            }

            throw Error(first, "Can only join GT_GLUE or GLUED_GT_GT");
        }

        Token Advance()
        {
            Previous = Tokens.Dequeue();
            return Previous;
        }

        Token AdvanceTwiceJoin()
        {
            Token first = Tokens.Dequeue();
            Token second = Tokens.Dequeue();
            Previous = JoinTogether(first, second);
            return Previous;
        }

        Token PeekNext()
        {
            EnsureTokens(2);
            return Tokens.ElementAt(1);
        }

        Token Peek()
        {
            EnsureTokens(1);
            return Tokens.Peek();
        }

        Token ConsumeAny(string message, params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                    return Advance();
            }
            throw Error(Peek(), message);
        }

        Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        bool CheckAny(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type)) return true;
            }
            return false;
        }

        bool Check(TokenType type)
        {
            if (IsAtEnd() && type != EOF) return false;
            if (IsAtEnd() && type == EOF) return true;
            return Peek().Type == type;
        }

        bool MatchNextJoin(TokenType first, TokenType second)
        {
            if (CheckNext(first, second))
            {
                AdvanceTwiceJoin();
                return true;
            }
            return false;
        }

        bool CheckNext(TokenType first, TokenType second)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == first && PeekNext().Type == second;
        }

        bool IsAtEnd() => Peek().Type == EOF;

        ParseError Error(Token token, string message)
        {
            Compiler.Error(token, message);
            return new ParseError();
        }
    }
}
