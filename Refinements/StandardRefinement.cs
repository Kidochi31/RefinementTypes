using RefinementTypes.Scanning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinementTypes.Refinements
{
    internal abstract class StandardRefinement
    {
        public abstract StandardRefinement Invert();

        public enum ComparisonType
        {
            EQUAL,
            NOT_EQUAL,
            GREATER_THAN,
            GREATER_THAN_OR_EQUAL,
            LESS_THAN,
            LESS_THAN_OR_EQUAL
        }

        internal class BaseTypeRefinement : StandardRefinement
        {
            public BaseType Type;
            public bool Not;

            public BaseTypeRefinement(BaseType type, bool not)
            {
                Type = type;
                Not = not;
            }

            public override StandardRefinement Invert()
            {
                return new BaseTypeRefinement(Type, !Not);
            }
        }

        public class Literal(LiteralToken token, bool not) : StandardRefinement
        {
            public readonly LiteralToken Token = token;
            public readonly bool Not = not;

            public override StandardRefinement Invert()
            {
                return new Literal(Token, !Not);
            }
        }

        public class Identifier(IdentifierToken token, bool not) : StandardRefinement
        {
            public readonly IdentifierToken Token = token;
            public readonly bool Not = not;

            public override StandardRefinement Invert()
            {
                return new Identifier(Token, !Not);
            }
        }

        public class Comparison(Token token, ComparisonType comparisonType, StandardRefinement right) : StandardRefinement
        {
            public readonly Token Operator = token;
            public readonly ComparisonType ComparisonType = comparisonType;
            public readonly StandardRefinement Right = right;

            public override StandardRefinement Invert()
            {
                ComparisonType comparison = ComparisonType switch
                {
                    ComparisonType.EQUAL => ComparisonType.NOT_EQUAL,
                    ComparisonType.NOT_EQUAL => ComparisonType.EQUAL,
                    ComparisonType.GREATER_THAN => ComparisonType.LESS_THAN_OR_EQUAL,
                    ComparisonType.LESS_THAN_OR_EQUAL => ComparisonType.GREATER_THAN,
                    ComparisonType.LESS_THAN => ComparisonType.GREATER_THAN_OR_EQUAL,
                    ComparisonType.GREATER_THAN_OR_EQUAL => ComparisonType.LESS_THAN,
                    _ => ComparisonType.EQUAL,
                };
                return new Comparison(Operator, comparison, right);
            }
        }

        public class Unary(Token token, StandardRefinement right, bool not) : StandardRefinement
        {
            public readonly Token Operator = token;
            public readonly StandardRefinement Right = right;
            public readonly bool Not = not;

            public override StandardRefinement Invert()
            {
                return new Unary(Operator, Right, !Not);
            }
        }

        public class Binary(Token token, StandardRefinement left, StandardRefinement right, bool not) : StandardRefinement
        {
            public readonly Token Operator = token;
            public readonly StandardRefinement Right = right;
            public readonly StandardRefinement Left = left;
            public readonly bool Not = not;

            public override StandardRefinement Invert()
            {
                return new Binary(Operator, Left, Right, !Not);
            }
        }


        public static List<List<StandardRefinement>> AndRefinements(List<List<StandardRefinement>> left, List<List<StandardRefinement>> right)
        {
            List<List<StandardRefinement>> refinements = [];
            foreach (List<StandardRefinement> lRefinements in left)
            {
                foreach (List<StandardRefinement> rRefinements in right)
                {
                    List<StandardRefinement> conjunction = [.. lRefinements, .. rRefinements];
                    refinements.Add(conjunction);
                }
            }
            return refinements;
        }

        public static List<List<StandardRefinement>> OrRefinements(List<List<StandardRefinement>> left, List<List<StandardRefinement>> right)
        {
            return [.. left, .. right];
        }

        public static List<List<StandardRefinement>> InvertRefinements(List<List<StandardRefinement>> refinements)
        {
            List<List<StandardRefinement>> sumOfProducts = InvertRefinementProduct(refinements[0]);
            foreach (List<StandardRefinement> products in refinements[1..])
            {
                sumOfProducts = AndRefinements(sumOfProducts, InvertRefinementProduct(products));
            }
            return sumOfProducts;
        }

        static List<List<StandardRefinement>> InvertRefinementProduct(List<StandardRefinement> refinementProduct)
        {
            return (from refinement in refinementProduct select new List<StandardRefinement>() { refinement.Invert() }).ToList();
        }
    }

    

    
}
