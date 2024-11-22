using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefinementTypes.Refinements;

namespace RefinementTypes
{
    internal class StandardBaseType
    {
        public List<StandardRefinement> Refinements;

        public StandardBaseType(List<StandardRefinement> refinements)
        {
            Refinements = refinements;
        } 
    }


}
