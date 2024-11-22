using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefinementTypes.Refinements;

namespace RefinementTypes
{
    internal static class TypeSimplifier
    {
        public static StandardType SimplifyType(Type type)
        {
            switch(type)
            {
                case BaseType baseType:
                    StandardRefinement typeRefinement = new BaseTypeRefinement(baseType);
                    return new StandardType([new StandardBaseType([typeRefinement])]);

                case RefinedType refinedType:
                    return null;
                    //return StandardType.Refine(SimplifyType(refinedType.BaseType), refinedType.Refinement);

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
    }
}
