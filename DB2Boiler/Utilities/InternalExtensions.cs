using IBM.Data.Db2;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB2Boiler.Utilities
{
    internal static class InternalExtensions
    {
        internal static string ToJsonParameters(this List<DB2Parameter> db2Parameters)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append('{');
            foreach (var db2Parameter in db2Parameters)
            {
                stringBuilder.Append($"{Environment.NewLine}    \"{db2Parameter.ParameterName}\":\"{db2Parameter.Value}\",");
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1); //remove trailing comma
            stringBuilder.Append($"{Environment.NewLine}}}");

            return stringBuilder.ToString();
        }
    }
}
