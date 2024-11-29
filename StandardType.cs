using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefinementTypes.Refinements;

namespace RefinementTypes
{
    internal class StandardType
    {
        public List<StandardBaseType> Bases;

        public StandardType()
        {
            Bases = [new StandardBaseType([])];
        }

        public StandardType(List<StandardBaseType> bases)
        {
            Bases = bases;
        }

        public List<List<StandardRefinement>> GetSumOfProductsRefinements()
        {
            return (from baseType in Bases select (from refinement in baseType.Refinements select refinement).ToList()).ToList();
        }

        public static StandardType Or(StandardType a, StandardType b)
        {
            return new StandardType([.. a.Bases, .. b.Bases]);
        }

        public static StandardType Refine(StandardType a, List<List<StandardRefinement>> sumOfProductsRefinements)
        {
            List<StandardBaseType> baseTypes = [];
            foreach (StandardBaseType aBase in a.Bases)
            {
                foreach(List<StandardRefinement> refinements in sumOfProductsRefinements)
                {
                    baseTypes.Add(new StandardBaseType([.. aBase.Refinements, .. refinements]));
                }
            }
            return new StandardType(baseTypes);
        }

        public static StandardType And(StandardType a, StandardType b)
        {
            List<StandardBaseType> baseTypes = [];
            foreach (StandardBaseType aBase in a.Bases)
            {
                foreach(StandardBaseType bBase in b.Bases)
                {
                    StandardBaseType conjunction = new StandardBaseType([.. aBase.Refinements, .. bBase.Refinements]);
                    baseTypes.Add(conjunction);
                }
            }
            return new StandardType(baseTypes);
        }
    }
}
