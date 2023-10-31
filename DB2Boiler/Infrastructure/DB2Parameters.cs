using IBM.Data.Db2;
using Microsoft.Azure.Functions.Worker.Http;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
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
                ActivateParametersWithValues();
            }

            if(parameterValues.Count > 0)
            {
                MapProvidedParameterValues(parameterValues);
            }

            return MapThisToListOfDB2Parameter();
        }

        private void ActivateParametersWithValues()
        {
            foreach (var parameter in GetType().GetProperties())
            {
                if (parameter.GetValue(this, null) != null)
                {
                    var attribute = (DB2ParamAttribute?)Attribute.GetCustomAttribute(parameter, typeof(DB2ParamAttribute));
                    attribute!.Activated = true;
                }
            }
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
                        var attribute = (DB2ParamAttribute?)Attribute.GetCustomAttribute(parameter, typeof(DB2ParamAttribute));
                        attribute!.Activated = true;
                    }
                }
            }
        }
        public virtual List<DB2Parameter> MapThisToListOfDB2Parameter()
        {
            var parameterList = new List<DB2Parameter>();
            foreach (var parameter in GetType().GetProperties().Where(p => p.GetValue(this, null) != null))
            {
                var db2ParameterAttribute = (DB2ParamAttribute?)Attribute.GetCustomAttribute(parameter, typeof(DB2ParamAttribute));
                if (db2ParameterAttribute != null)
                {
                    parameterList.Add(CreateParameter(parameter, db2ParameterAttribute));
                }
            }
            return parameterList;
        }

        public virtual DB2Parameter CreateParameter(PropertyInfo propertyInfo, DB2ParamAttribute db2ParamAttribute, string? defaultValue = null)
        {
            var newDB2Parameter = new DB2Parameter(propertyInfo!.Name, defaultValue ?? propertyInfo.GetValue(this, null));
            newDB2Parameter.Direction = db2ParamAttribute!.Direction;
            newDB2Parameter.DB2Type = db2ParamAttribute.DB2Type;
            if (db2ParamAttribute.Size != 0)
            {
                newDB2Parameter.Size = db2ParamAttribute.Size;
            }

            return newDB2Parameter;
        }
    }
}
