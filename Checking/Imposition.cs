using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefinementTypes.Execution;
using RefinementTypes.Refinements;

namespace RefinementTypes.Checking
{
    internal static class Imposition
    {
        public static bool AFitsIntoB(StandardType a, StandardType b)
        {
            // A fits into B iff A U (not B) = 0
            List<List<StandardRefinement>> aRefinements = a.GetSumOfProductsRefinements();
            List<List<StandardRefinement>> bRefinements = b.GetSumOfProductsRefinements();
            List<List<StandardRefinement>> invertedBRefinement = StandardRefinement.InvertRefinements(bRefinements);

            //Console.WriteLine("");
            //Console.WriteLine("Inverted B:");
            //Console.WriteLine(ASTPrinter.StringifyStandardType(new StandardType((from l in invertedBRefinement select new StandardBaseType(l)).ToList())));

            if(Contradiction.RefinementSOPSelfContradicts(aRefinements, []))
            {
                Console.WriteLine("");
                Console.WriteLine($"Type A contradicts itself: {ASTPrinter.StringifyStandardType(a)}");
            }

            // Here, if B = (a b) | (c d), then (not B) = (a' | b') & (c' | d') = a'c' | a'd' | b'c' | b'd'
            // It must be shown that A & (not B) is a contradiction on all of these, AND on all cases of A
            // So, algorithm is to go through all products of A, and all products of inverted B, and check for contradiction.
            foreach(List<StandardRefinement> aProduct in aRefinements)
            {
                foreach(List<StandardRefinement> bProduct in invertedBRefinement)
                {
                    if (!Contradiction.RefinementProductAContradictsB(aProduct, bProduct, []) && !Contradiction.RefinementProductSelfContradicts(bProduct, [.. aProduct]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        

        
    }
}
