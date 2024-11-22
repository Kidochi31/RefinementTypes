using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinementTypes.Refinements
{
    internal abstract class StandardRefinement
    {
    }

    internal class BaseTypeRefinement : StandardRefinement
    {
        public BaseType Type;

        public BaseTypeRefinement(BaseType type)
        {
            Type = type;
        }
    }
}
