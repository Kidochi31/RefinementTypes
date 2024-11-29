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

    internal class Print(Token printToken, IdentifierToken name, Token newLine) : TopLevel
    {
        public Token PrintToken = printToken;
        public IdentifierToken Name = name;
        public Token NewLine = newLine;
    }

    internal class TypeFit(Token fitToken, Type fromType, Token inToken, Type inType, Token newLine) : TopLevel
    {
        public Token FitToken = fitToken;
        public Type FromType = fromType;
        public Type InType = inType;
        public Token NewLine = newLine;
        public Token InToken = inToken;
    }

    internal class TypeDeclaration(Token typeToken, IdentifierToken typeName, Token? colonToken, Type? baseType, Token newLine) : TopLevel
    {
        public Token TypeToken = typeToken;
        public IdentifierToken TypeName = typeName;
        public Token NewLine = newLine;
        public Token? ColonToken = colonToken;
        public Type? BaseType = baseType;
    }

    internal class TypeTest(Token testToken, Type type, Token newLine) : TopLevel
    {
        public Token TestToken = testToken;
        public Type Type = type;
        public Token NewLine = newLine;
    }
}
