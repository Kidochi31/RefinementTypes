using RefinementTypes.Scanning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinementTypes.Refinements
{
    internal abstract class Refinement
    {
        public class Literal(LiteralToken token) : Refinement
        {
            public readonly LiteralToken Token = token;
        }

        public class Identifier(IdentifierToken token) : Refinement
        {
            public readonly IdentifierToken Token = token;

        }

        public class Unary(Token token, Refinement right) : Refinement
        {
            public readonly Token Operator = token;
            public readonly Refinement Right = right;
        }

        public class Binary(Token token, Refinement left, Refinement right) : Refinement
        {
            public readonly Token Operator = token;
            public readonly Refinement Right = right;
            public readonly Refinement Left = left;
        }

        public class Grouping(Token paren1, Token paren2, Refinement refinement) : Refinement
        {
            public readonly Refinement Refinement = refinement;
        }
    }
}
