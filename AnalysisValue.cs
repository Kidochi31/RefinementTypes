using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace RefinementTypes
{
    internal class AnalysisValue(object value, Type type)
    {
        public object Value = value;
        public Type Type = type;

        public override string ToString()
        {
            return $"Value({Type}, {Value})";
        }
    }
}
