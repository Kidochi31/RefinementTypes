using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinementTypes
{
    internal class StandardBaseType
    {
        public List<Refinement> Refinements;

        public StandardBaseType(List<Refinement> refinements)
        {
            Refinements = refinements;
        } 
    }
}
