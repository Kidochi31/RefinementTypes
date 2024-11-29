using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinementTypes
{
    internal static class Environment
    {
        static Dictionary<string, AnalysisValue> Values = new();

        static  Environment()
        {
            Values.Add("Any", new AnalysisValue(NamedType.Any, NamedType.Type));
            Values.Add("Type", new AnalysisValue(NamedType.Type, NamedType.Type));
        }

        public static void SetValue(string key, AnalysisValue value)
        {
            Values[key] = value;
        }

        public static AnalysisValue GetValue(string key)
        {
            if (Values.ContainsKey(key)) {
                return Values[key];
            }

            throw new Exception($"no value with name {key} found");
        }
    }
}
