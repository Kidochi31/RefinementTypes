using RefinementTypes.Refinements;
using RefinementTypes.Scanning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RefinementTypes.Scanning.TokenType;

namespace RefinementTypes.Parsing
{
    internal partial class Parser
    {
        // Type -> CombinationType ;
        Type Type()
        {
            return CombinationType();
        }

        // CombinationType -> RefinedType ( '|' RefinedType )*
		//                  | RefinedType( '&' RefinedType )* ;
        Type CombinationType()
        {
            Type first = RefinedType();
            if (Check(BAR))
            {
                List<Type> bases = ParseCombinationTypeWith(first, BAR);
                return new OrType(bases);
            }
            if (Check(AMPERSAND))
            {
                List<Type> bases = ParseCombinationTypeWith(first, AMPERSAND);
                return new AndType(bases);
            }
            return first;
        }

        List<Type> ParseCombinationTypeWith(Type first, TokenType token)
        {
            List<Type> bases = [first];
            while (Match(BAR, AMPERSAND))
            {
                if (Previous.Type != token)
                    Error(Previous, "Cannot mix & and | without parentheses.");
                Type next = RefinedType();
                bases.Add(next);
            }
            return bases;
        }

        // RefinedType -> BaseType ( '[' RefinementExpression ']' ) * ;
        Type RefinedType()
        {
            Type baseType = BaseType();
            while(Match(LEFT_SQUARE))
            {
                Refinement refinement = Refinement();
                baseType = new RefinedType(baseType, refinement);
                Consume(RIGHT_SQUARE, "Expected ']' after '['.");
            }
            return baseType;
        }

        // BaseType -> Name | '(' Type ')' ;
        Type BaseType()
        {
            if (Match(IDENTIFIER))
            {
                IdentifierToken identifier = (IdentifierToken)Previous;
                return new BaseType(identifier.IdentifierName);
            }
            if (Match(LEFT_PAREN))
            {
                Type type = Type();
                Consume(RIGHT_PAREN, "Expected ')' after '('.");
                return new GroupType(type);
            }
            throw Error(Peek(), "Expected identifier for type.");
        }


        //RefinementExpression -> LogicalRefinementExpression ;
        Refinement Refinement()
        {
            return RefinementLogicalBinary();
        }

        //LogicalRefinementExpression -> LogicalUnary( 'and' LogicalUnary ) *
        //                             | LogicalUnary( 'or' LogicalUnary ) *
        //                             | LogicalUnary( 'xor' LogicalUnary ) * ;
        Refinement RefinementLogicalBinary()
        {
            Refinement left = RefinementLogicalUnary();
            //and, or, xor can only chain with themselves
            if (CheckAny(AND, OR, XOR))
            {
                TokenType type = Peek().Type;
                //match and, or, and xor
                while (Match(AND, OR, XOR))
                {
                    Token previous = Previous;
                    Refinement right = RefinementLogicalUnary();
                    left = new Refinement.Binary(previous, left, right);

                    if (previous.Type != type)
                    {
                        Error(previous, "Logical operators can only chain with themselves.");
                    }
                }
            }

            return left;
        }


        //LogicalUnary -> LogicalNot | Comparison ;
        //LogicalNot -> 'not' LogicalNot | Prefix ;
        Refinement RefinementLogicalUnary()
        {
            if (Match(NOT))
            {
                Token token = Previous;
                Refinement right = RefinementLogicalUnary();
                //if not is not followed by a prefix expression (or another not)
                //then this is invalid
                if (right is not Refinements.Refinement.Literal
                    && right is not Refinements.Refinement.Unary
                    && right is not Refinements.Refinement.Grouping)
                {
                    Error(token, "not must be followed by a not or prefix expression.");
                }

                return new Refinement.Unary(token, right);
            }
            return RefinementComparison();
        }

        //Comparison -> | ('=' | '!=' | '>' | '<=' | '>' | '>=') Numeric ;
        Refinement RefinementComparison()
        {
            if (Match(EQUALS, BANG_EQUALS, LESS, LT_EQUALS)
                    || MatchNextJoin(GT_GLUE, EQUALS)
                    || Match(GT_GLUE, GREATER))
            {
                Token token = Previous;
                Refinement right = RefinementNumeric();
                return new Refinement.Unary(token, right);
            }

            return RefinementNumeric();
        }

        //Numeric -> Term ;
        Refinement RefinementNumeric()
        {
            return RefinementTerm();
        }

        //Term -> Factor(( '+' | '-' ) Factor )* ;

        Refinement RefinementTerm()
        {
            Refinement left = RefinementFactor();
            while (Match(PLUS, MINUS))
            {
                Token previous = Previous;
                Refinement right = RefinementFactor();
                left = new Refinement.Binary(previous, left, right);
            }

            return left;
        }

        //Factor -> Prefix ;
        Refinement RefinementFactor()
        {
            return RefinementPrefix();
        }

        //Prefix -> ( '+' | '-' ) Prefix | Primary ;
        Refinement RefinementPrefix()
        {
            if (Match(PLUS, MINUS))
            {
                Token token = Previous;
                Refinement right = RefinementPrefix();
                return new Refinement.Unary(token, right);
            }

            return RefinementPrimary();
        }

        //Primary -> IDENTIFIER | Literal | '(' RefinementExpression ')' ;
        //Literal -> 'true' | 'false' | CHAR_LITERAL | NUMBER_LITERAL ;
        Refinement RefinementPrimary()
        {
            if (Match(TRUE)) return new Refinement.Literal((LiteralToken)Previous);
            if (Match(FALSE)) return new Refinement.Literal((LiteralToken)Previous);
            if (Match(IDENTIFIER)) return new Refinement.Identifier((IdentifierToken)Previous);
            if (Match(DECIMAL_FLOAT, DECIMAL_INTEGER)) return new Refinement.Literal((LiteralToken)Previous);
            if (Match(CHAR)) return new Refinement.Literal((LiteralToken)Previous);

            if (Match(LEFT_PAREN))
            {
                Token paren1 = Previous;
                Refinement expr = Refinement();
                Consume(RIGHT_PAREN, "Expected ')' after refinement expression.");
                Token paren2 = Previous;
                return new Refinement.Grouping(paren1, paren2, expr);
            }
            throw Error(Peek(), "Expected refinement expression.");
        }
    }
}
