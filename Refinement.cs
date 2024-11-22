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

    internal class BaseTypeRefinement : Refinement
    {
        public BaseType Type;

        public BaseTypeRefinement(BaseType type)
        {
            Type = type;
        }
    }
}
