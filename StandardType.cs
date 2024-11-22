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

        public StandardType(List<StandardBaseType> bases)
        {
            Bases = bases;
        }

        public static StandardType Or(StandardType a, StandardType b)
        {
            return new StandardType([.. a.Bases, .. b.Bases]);
        }

        public static StandardType Refine(StandardType a, StandardRefinement refinement)
        {
            return new StandardType((from baseType in a.Bases
                                     select new StandardBaseType([.. baseType.Refinements, refinement])).ToList());
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
