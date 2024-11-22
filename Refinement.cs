using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinementTypes
{
    internal abstract class Refinement
    {
    }

    internal class TypeRefinement : Refinement
    {
        public BaseType Type;

        public TypeRefinement(BaseType type)
        {
            Type = type;
        }
    }
}
