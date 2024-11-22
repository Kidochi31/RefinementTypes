using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinementTypes
{
    internal abstract class Type
    {

    }

    internal class BaseType : Type
    {
        public string Name { get; set; }

        public BaseType(string name)
        {
            Name = name;
        }

        public static BaseType AnyType = new BaseType("Any");
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
    }

    internal class OrType : Type
    {
        public List<Type> BaseTypes;

        public OrType(List<Type> baseTypes)
        {
            BaseTypes = baseTypes;
        }
    }

    internal class AndType : Type
    {
        public List<Type> BaseTypes;

        public AndType(List<Type> baseTypes)
        {
            BaseTypes = baseTypes;
        }
    }
}
