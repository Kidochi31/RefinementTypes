using RefinementTypes.Scanning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinementTypes.Parsing
{
    internal abstract class TopLevel
    {
    }

    internal class TypeDeclaration(Token typeToken, IdentifierToken typeName, Token newLine) : TopLevel
    {
        public Token TypeToken = typeToken;
        public IdentifierToken TypeName = typeName;
        public Token NewLine = newLine;
    }

    internal class TypeTest(Token testToken, Type type, Token newLine) : TopLevel
    {
        public Token TestToken = testToken;
        public Type Type = type;
        public Token NewLine = newLine;
    }
}
