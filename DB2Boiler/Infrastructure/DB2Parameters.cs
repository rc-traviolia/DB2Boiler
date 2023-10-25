using IBM.Data.Db2;
using Microsoft.Azure.Functions.Worker.Http;
using System.Data;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;

namespace DB2Boiler.Infrastructure
{

    public abstract class DB2Parameters : IDB2Parameters
    {
        public virtual List<DB2Parameter> MapAndRetrieveParameters(HttpRequestData? httpRequestData, List<(string Name, string Value)> parameterValues)
        {
            if(httpRequestData != null)
            {
                MapHttpRequestDataToParameters(httpRequestData);
            }

            if(parameterValues.Count > 0)
            {
                MapProvidedParameterValues(parameterValues);
            }

            return MapThisToListOfDB2Parameter();
        }

        public abstract void MapHttpRequestDataToParameters(HttpRequestData httpRequestData);
        public virtual void MapProvidedParameterValues(List<(string Name, string Value)> parameterValues)
        {
            foreach (var parameter in GetType().GetProperties())
            {
                foreach (var parameterValue in parameterValues)
                {
                    if (parameterValue.Name == parameter.Name)
                    {
                        parameter.SetValue(this, parameterValue.Value);
                    }
                }
            }
        }
        public virtual List<DB2Parameter> MapThisToListOfDB2Parameter()
        {
            var parameterList = new List<DB2Parameter>();
            foreach (var parameter in GetType().GetProperties().Where(parameter => parameter.GetValue(this, null) != null || Attribute.GetCustomAttribute(parameter, typeof(DB2ParamAttribute)) != null))
            {
                //Skip all non-DB2Param properties
                var parameterDirectionAttribute = (DB2ParamAttribute?)Attribute.GetCustomAttribute(parameter, typeof(DB2ParamAttribute));
                var newDb2Parameter = new DB2Parameter(parameter.Name, parameter.GetValue(this, null));
                newDb2Parameter.Direction = parameterDirectionAttribute!.Direction;
                newDb2Parameter.DB2Type = parameterDirectionAttribute.DB2Type;
                if (parameterDirectionAttribute.Size != 0)
                {
                    newDb2Parameter.Size = parameterDirectionAttribute.Size;
                }
                parameterList.Add(newDb2Parameter);
            }
            return parameterList;
        }
    }
}
