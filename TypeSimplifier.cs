using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinementTypes
{
    internal static class TypeSimplifier
    {
        public static StandardType SimplifyType(Type type)
        {
            switch(type)
            {
                case BaseType baseType:
                    Refinement typeRefinement = new TypeRefinement(baseType);
                    return new StandardType([new StandardBaseType([typeRefinement])]);

                case RefinedType refinedType:
                    return StandardType.Refine(Simplify(refinedType.BaseType), refinedType.Refinement);

                case OrType orType:
                    StandardType orResultType = Simplify(orType.BaseTypes[0]);
                    foreach (Type orBase in orType.BaseTypes[1..])
                    {
                        orResultType = StandardType.Or(orResultType, Simplify(orBase));
                    }
                    return orResultType;

                case AndType andType:
                    StandardType andResultType = Simplify(andType.BaseTypes[0]);
                    foreach (Type andBase in andType.BaseTypes[1..])
                    {
                        andResultType = StandardType.And(andResultType, Simplify(andBase));
                    }
                    return andResultType;
            }
            Console.WriteLine($"Invalid type: {type}!");
            return null;
        }
    }
}
