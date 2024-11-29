using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RefinementTypes.Refinements;
using static RefinementTypes.Scanning.TokenType;

namespace RefinementTypes
{
    internal static class TypeSimplifier
    {
        public static StandardType SimplifyType(Type type)
        {
            switch(type)
            {
                case NamedType baseType:
                    if (baseType.Equals(NamedType.Any))
                        return new StandardType();

                    StandardRefinement typeRefinement = new StandardRefinement.BaseTypeRefinement(baseType, false);
                    return StandardType.Refine(SimplifyType(baseType.BaseType), [[typeRefinement]]);

                case RefinedType refinedType:
                    List<List<StandardRefinement>> sumOfProducts = ConvertRefinementToSumOfProducts(refinedType.Refinement, false);
                    return StandardType.Refine(SimplifyType(refinedType.BaseType), sumOfProducts);

                case OrType orType:
                    StandardType orResultType = SimplifyType(orType.BaseTypes[0]);
                    foreach (Type orBase in orType.BaseTypes[1..])
                    {
                        orResultType = StandardType.Or(orResultType, SimplifyType(orBase));
                    }
                    return orResultType;

                case AndType andType:
                    StandardType andResultType = SimplifyType(andType.BaseTypes[0]);
                    foreach (Type andBase in andType.BaseTypes[1..])
                    {
                        andResultType = StandardType.And(andResultType, SimplifyType(andBase));
                    }
                    return andResultType;

                case GroupType groupType:
                    return SimplifyType(groupType.BaseType);
            }
            Console.WriteLine($"Invalid type: {type}!");
            return null;
        }

        static List<List<StandardRefinement>> ConvertRefinementToSumOfProducts(Refinement refinement, bool not)
        {
            switch(refinement)
            {
                case Refinement.Literal literal:
                    return [[new StandardRefinement.Literal(literal.Token, not)]];
                case Refinement.Identifier identifier:
                    return [[new StandardRefinement.Identifier(identifier.Token, not)]];
                case Refinement.LogicalUnary lunary: // not
                    return ConvertRefinementToSumOfProducts(lunary.Right, !not);
                case Refinement.Unary unary: // ('=' | '!=' | '>' | '<=' | '>' | '>=')
                    return [[CreateStandardUnaryRefinement(unary, not)]];
                case Refinement.LogicalBinary lbinary:
                    return CreateLogicalBinaryRefinement(lbinary, not);
                case Refinement.Binary binary:
                    return [[CreateStandardBinaryRefinement(binary, not)]];
                case Refinement.Grouping grouping:
                    return ConvertRefinementToSumOfProducts(grouping.Refinement, not);
                case Refinement.Is ris:
                    StandardType standardType = SimplifyType(ris.Type);
                    List<List<StandardRefinement>> refinements = standardType.GetSumOfProductsRefinements();
                    if (not)
                        refinements = StandardRefinement.InvertRefinements(refinements);
                    return refinements;

            }
            throw new Exception("Invalid refinement, cannot be converted to sum of products");
        }

        

        static List<List<StandardRefinement>> CreateLogicalBinaryRefinement(Refinement.LogicalBinary binary, bool not)
        {
            List<List<StandardRefinement>> left = ConvertRefinementToSumOfProducts(binary.Left, not);
            List<List<StandardRefinement>> right = ConvertRefinementToSumOfProducts(binary.Right, not);
            switch (binary.Operator.Type)
            {
                case AND:
                    if (!not) // and together
                        return StandardRefinement.AndRefinements(left, right);
                    else // or together
                    {
                        return StandardRefinement.OrRefinements(left, right);
                    }
                case OR:
                    if (!not) // or together
                        return StandardRefinement.OrRefinements(left, right);

                    else // and together
                    {
                        return StandardRefinement.AndRefinements(left, right);
                    }
                case XOR:
                    List<List<StandardRefinement>> leftnot = ConvertRefinementToSumOfProducts(binary.Left, !not);
                    List<List<StandardRefinement>> rightnot = ConvertRefinementToSumOfProducts(binary.Right, !not);
                    if (!not) // (left & rightnot) | (leftnot & right)
                    {
                        return StandardRefinement.OrRefinements(
                            StandardRefinement.AndRefinements(left, rightnot),
                            StandardRefinement.AndRefinements(leftnot, right));
                    }
                    else // (leftnot & rightnot) | (left & right)
                    {
                        return StandardRefinement.OrRefinements(
                            StandardRefinement.AndRefinements(leftnot, rightnot),
                            StandardRefinement.AndRefinements(left, right));
                    }
            }
            throw new Exception("Invalid logical binary operator");
        }

        

        static StandardRefinement CreateStandardBinaryRefinement(Refinement.Binary binary, bool not)
        {
            // None yet
            StandardRefinement left = CreateNonLogicalStandardRefinement(binary.Left);
            StandardRefinement right = CreateNonLogicalStandardRefinement(binary.Right);
            switch (binary.Operator.Type)
            {
            }
            throw new Exception($"Invalid binary operator in logical context {binary.Operator.Lexeme}.");
        }

        static StandardRefinement CreateStandardUnaryRefinement(Refinement.Unary unary, bool not)
        {
            // ('=' | '!=' | '>' | '<=' | '>' | '>=')
            StandardRefinement right = CreateNonLogicalStandardRefinement(unary.Right);
            switch(unary.Operator.Type)
            {
                case EQUALS:
                    return new StandardRefinement.Comparison(unary.Operator, 
                        not ? StandardRefinement.ComparisonType.NOT_EQUAL : StandardRefinement.ComparisonType.EQUAL, right);
                case BANG_EQUALS:
                    return new StandardRefinement.Comparison(unary.Operator,
                        not ? StandardRefinement.ComparisonType.EQUAL : StandardRefinement.ComparisonType.NOT_EQUAL, right);
                case GT_GLUE:
                case GREATER:
                    return new StandardRefinement.Comparison(unary.Operator,
                        not ? StandardRefinement.ComparisonType.LESS_THAN_OR_EQUAL : StandardRefinement.ComparisonType.GREATER_THAN, right);
                case GLUED_GT_EQUALS:
                    return new StandardRefinement.Comparison(unary.Operator,
                        not ? StandardRefinement.ComparisonType.LESS_THAN : StandardRefinement.ComparisonType.GREATER_THAN_OR_EQUAL, right);
                case LESS:
                    return new StandardRefinement.Comparison(unary.Operator,
                        not ? StandardRefinement.ComparisonType.GREATER_THAN_OR_EQUAL : StandardRefinement.ComparisonType.LESS_THAN, right);
                case LT_EQUALS:
                    return new StandardRefinement.Comparison(unary.Operator,
                        not ? StandardRefinement.ComparisonType.GREATER_THAN : StandardRefinement.ComparisonType.LESS_THAN_OR_EQUAL, right);
            }
            throw new Exception($"Invalid unary operator in logical context {unary.Operator.Lexeme}.");
        }

        static StandardRefinement CreateNonLogicalStandardRefinement(Refinement refinement)
        {
            switch (refinement)
            {
                case Refinement.Literal literal:
                    return new StandardRefinement.Literal(literal.Token, false);
                case Refinement.Identifier identifier:
                    return new StandardRefinement.Identifier(identifier.Token, false);
                case Refinement.Unary unary: // also Refinement.LogicalUnary
                    return new StandardRefinement.Unary(unary.Operator, CreateNonLogicalStandardRefinement(unary.Right), false);
                case Refinement.Binary binary: // also Refinement.LogicalBinary
                    return new StandardRefinement.Binary(binary.Operator,
                        CreateNonLogicalStandardRefinement(binary.Left),
                        CreateNonLogicalStandardRefinement(binary.Right), false);
                case Refinement.Grouping grouping:
                    return CreateNonLogicalStandardRefinement(grouping.Refinement);
                case Refinement.Is ris:
                    Compiler.Error(ris.Colon, "Cannot create a non-logical standard refinement for ':'");
                    throw new Exception("No non-logical ':'");
            }
            throw new Exception("Invalid refinement");
        }
    }
}
