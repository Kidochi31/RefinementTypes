﻿using RefinementTypes.Parsing;
using RefinementTypes.Refinements;
using RefinementTypes.Scanning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static RefinementTypes.Refinements.Refinement;

namespace RefinementTypes.Printing
{
    internal class ASTPrinter
    {
        public static void PrintTopLevel(TopLevel topLevel)
        {
            switch(topLevel)
            {
                case TypeDeclaration typeDeclaration:
                    Console.WriteLine($"Type Declaration: {typeDeclaration.TypeName.IdentifierName}");
                    return;
                case TypeTest typeTest:
                    Console.WriteLine($"Type Test: {StringifyType(typeTest.Type)}");
                    //StandardType standardType = TypeSimplifier.SimplifyType(typeTest.Type);
                    //Console.WriteLine($"Standardised Type: {StringifyStandardType(standardType)}");
                    return;
            }
        }

        public static string StringifyStandardType(StandardType type)
        {
            string result = StringifyStandardBaseType(type.Bases[0]);
            foreach (StandardBaseType baseType in type.Bases[1..])
            {
                result += $" | {StringifyStandardBaseType(baseType)}";
            }
            return result;
        }

        public static string StringifyStandardBaseType(StandardBaseType type)
        {
            string result = "Any";
            foreach(StandardRefinement refinement in type.Refinements)
            {
                result += $"[{StringifyStandardRefinement(refinement)}]";
            }
            return result;
        }

        public static string StringifyType(Type type)
        {
            switch(type)
            {
                case BaseType baseType:
                    return baseType.Name;

                case RefinedType refinedType:
                    return $"{StringifyType(refinedType.BaseType)}[{StringifyRefinement(refinedType.Refinement)}]";

                case OrType orType:
                    string orString = StringifyType(orType.BaseTypes[0]);
                    foreach (Type orBase in orType.BaseTypes[1..])
                    {
                        orString += $" | {StringifyType(orBase)}";
                    }
                    return orString;

                case AndType andType:
                    string andString = StringifyType(andType.BaseTypes[0]);
                    foreach (Type andBase in andType.BaseTypes[1..])
                    {
                        andString += $" & {StringifyType(andBase)}";
                    }
                    return andString;

                case GroupType groupType:
                    return $"({StringifyType(groupType.BaseType)})";
            }

            return "INVALID TYPE";
        }

        public static string StringifyRefinement(Refinement refinement)
        {
            switch(refinement)
            {
                case Refinement.Literal literal:
                    return literal.Token.Lexeme;
                case Refinement.Identifier identifier:
                    return identifier.Token.IdentifierName;
                case Refinement.Grouping grouping:
                    return StringifyRefinement(grouping.Refinement);
                case Refinement.Unary unary:
                    return $"( {unary.Operator.Lexeme} {StringifyRefinement(unary.Right)} )";
                case Refinement.Binary binary:
                    return $"( {StringifyRefinement(binary.Left)} {binary.Operator.Lexeme} {StringifyRefinement(binary.Right)} )";
                case Refinement.Is ris:
                    return $"(: {StringifyType(ris.Type)} )";
            }

            return "INVALID REFINEMENT";
        }

        public static string StringifyStandardRefinement(StandardRefinement refinement)
        {
            switch (refinement)
            {
                case BaseTypeRefinement baseTypeRefinement:
                    return $": {baseTypeRefinement.Type.Name}";
            }

            return "INVALID STANDARD REFINEMENT";
        }
    }
}
