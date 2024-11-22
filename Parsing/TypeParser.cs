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
            return BaseType();
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
    }
}
