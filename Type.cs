using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using RefinementTypes.Execution;
using RefinementTypes.Refinements;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RefinementTypes
{
    internal abstract class Type
    {
        public override string ToString()
        {
            return ASTPrinter.StringifyType(this);
        }

        public abstract bool WillBeSubtypeOf(NamedType type);

        public abstract bool MayBeSubtypeOf(NamedType type);

        public abstract bool WontBeSubtypeOf(NamedType type);
    }

    internal class NamedType : Type
    {
        public string Name { get; set; }
        public Type BaseType { get; set; }

        private static NamedType CreateAnyType()
        {
            NamedType anyType = new NamedType("Any");
            anyType.BaseType = anyType;
            return anyType;
        }
        public NamedType(string name)
        {
            Name = name;
            BaseType = Any;
        }

        public NamedType(string name, Type baseType)
        {
            Name = name;
            BaseType = baseType;
        }

        public static NamedType Any = CreateAnyType();
        public static NamedType Type = new NamedType("Type");

        public override bool WillBeSubtypeOf(NamedType type)
        {
            if (Equals(type))
                return true;
            if (!Equals(Any))
            {
                return BaseType.WillBeSubtypeOf(type);
            }
            // Any
            return false;
        }

        public override bool MayBeSubtypeOf(NamedType type)
        {
            if (Equals(type))
                return true;
            if (!Equals(Any))
            {
                return BaseType.MayBeSubtypeOf(type);
            }
            // Any
            return false;
        }

        public override bool WontBeSubtypeOf(NamedType type)
        {
            if (Equals(type))
                return false;
            if (!Equals(Any))
            {
                return BaseType.WontBeSubtypeOf(type);
            }
            // Any
            return true;
        }
    }

    internal class RefinedType : Type
    {
        public Type BaseType;
        public Refinement Refinement;

        public RefinedType(Type baseType, Refinement refinement)
        {
            BaseType = baseType;
            Refinement = refinement;
        }

        public override bool WillBeSubtypeOf(NamedType type)
        {
            return BaseType.WillBeSubtypeOf(type);
        }

        public override bool MayBeSubtypeOf(NamedType type)
        {
            return BaseType.MayBeSubtypeOf(type);
        }

        public override bool WontBeSubtypeOf(NamedType type)
        {
            return BaseType.WontBeSubtypeOf(type);
        }
    }

    internal class OrType : Type
    {
        public List<Type> BaseTypes;

        public OrType(List<Type> baseTypes)
        {
            BaseTypes = baseTypes;
        }

        public override bool WillBeSubtypeOf(NamedType type)
        {
            foreach(Type baseType in BaseTypes)
            {
                if (!baseType.WillBeSubtypeOf(type))
                    return false;
            }
            return true;
        }

        public override bool MayBeSubtypeOf(NamedType type)
        {
            foreach (Type baseType in BaseTypes)
            {
                if (baseType.MayBeSubtypeOf(type))
                    return true;
            }
            return false;
        }

        public override bool WontBeSubtypeOf(NamedType type)
        {
            foreach (Type baseType in BaseTypes)
            {
                if (!baseType.WontBeSubtypeOf(type))
                    return false;
            }
            return true;
        }
    }

    internal class AndType : Type
    {
        public List<Type> BaseTypes;

        public AndType(List<Type> baseTypes)
        {
            BaseTypes = baseTypes;
        }

        public override bool WillBeSubtypeOf(NamedType type)
        {
            foreach (Type baseType in BaseTypes)
            {
                if (baseType.WillBeSubtypeOf(type))
                    return true;
            }
            return false;
        }

        public override bool MayBeSubtypeOf(NamedType type)
        {
            foreach (Type baseType in BaseTypes)
            {
                if (baseType.MayBeSubtypeOf(type))
                    return true;
            }
            return false;
        }

        public override bool WontBeSubtypeOf(NamedType type)
        {
            foreach (Type baseType in BaseTypes)
            {
                if (baseType.WontBeSubtypeOf(type))
                    return true;
            }
            return false;
        }
    }

    internal class GroupType : Type
    {
        public Type BaseType;

        public GroupType(Type baseType)
        {
            BaseType = baseType;
        }

        public override bool WillBeSubtypeOf(NamedType type)
        {
            return BaseType.WillBeSubtypeOf(type);
        }

        public override bool MayBeSubtypeOf(NamedType type)
        {
            return BaseType.MayBeSubtypeOf(type);
        }

        public override bool WontBeSubtypeOf(NamedType type)
        {
            return BaseType.WontBeSubtypeOf(type);
        }
    }
}
