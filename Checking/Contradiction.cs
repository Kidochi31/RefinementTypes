using RefinementTypes.Printing;
using RefinementTypes.Refinements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinementTypes.Checking
{
    internal static class Contradiction
    {
        public static bool RefinementSOPSelfContradicts(List<List<StandardRefinement>> refinements, List<StandardRefinement> context)
        {
            foreach(List<StandardRefinement> product in refinements)
            {
                if(!RefinementProductSelfContradicts(product, context))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool RefinementProductSelfContradicts(List<StandardRefinement> refinements, List<StandardRefinement> context)
        {
            foreach(StandardRefinement refinement1 in refinements)
            {
                foreach (StandardRefinement refinement2 in refinements)
                {
                    if (RefinementAContradictsB(refinement1, refinement2, context))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public static bool RefinementProductAContradictsB(List<StandardRefinement> a, List<StandardRefinement> b, List<StandardRefinement> context)
        {
            List<StandardRefinement> combined = [.. a, .. b, .. context];
            foreach (StandardRefinement aRefinement in a)
            {
                foreach (StandardRefinement bRefinement in b)
                {
                    if (RefinementAContradictsB(aRefinement, bRefinement, combined))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool RefinementAContradictsB(StandardRefinement a, StandardRefinement b, List<StandardRefinement> context)
        {
            //nsole.WriteLine($"Comparing {ASTPrinter.StringifyStandardRefinement(a)} with {ASTPrinter.StringifyStandardRefinement(b)}");

            switch(a)
            {
                case StandardRefinement.BaseTypeRefinement baseType:
                    return BaseTypeRefinementContradiction(baseType, b, context);
                    
            }
            Console.WriteLine("Invalid refinement found");
            return false;
        }

        static bool BaseTypeRefinementContradiction(StandardRefinement.BaseTypeRefinement a, StandardRefinement b, List<StandardRefinement> context)
        {
            if(b is StandardRefinement.BaseTypeRefinement baseType)
            {
                // a : X contradicts b : Y if X :/ Y and Y :/ X
                if (!a.Not && !baseType.Not)
                {
                    return !BaseType.AIsSubtypeOfB(a.Type, baseType.Type) && !BaseType.AIsSubtypeOfB(baseType.Type, a.Type);
                }
                // a :/ X never contradicts b :/ Y
                if (a.Not && baseType.Not)
                    return false;
                // a :/ X contradicts b: Y if Y : X
                if (a.Not && !baseType.Not)
                {
                    return BaseType.AIsSubtypeOfB(baseType.Type, a.Type);
                }
                // a : X contradicts b:/ Y if X : Y
                else
                {
                    return BaseType.AIsSubtypeOfB(a.Type, baseType.Type);
                }
            }
            else
            {
                return false;
            }
        }
    }
}
