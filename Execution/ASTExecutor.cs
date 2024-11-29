using RefinementTypes.Checking;
using RefinementTypes.Parsing;
using RefinementTypes.Refinements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinementTypes.Execution
{
    internal static class ASTExecutor
    {
        public static void ExecuteTopLevel(TopLevel topLevel)
        {
            switch (topLevel)
            {
                case TypeDeclaration typeDeclaration:
                    Console.WriteLine("Adding to environment");
                    Type typeDeclarationValue;
                    if (typeDeclaration.BaseType is null)
                    {
                        typeDeclarationValue = new NamedType(typeDeclaration.TypeName.IdentifierName);
                    }
                    else
                    {
                        typeDeclarationValue = new NamedType(typeDeclaration.TypeName.IdentifierName, typeDeclaration.BaseType);
                    }
                    Environment.SetValue(typeDeclaration.TypeName.IdentifierName, new AnalysisValue(typeDeclarationValue, NamedType.Type));
                    return;
            }
        }
    }
}
