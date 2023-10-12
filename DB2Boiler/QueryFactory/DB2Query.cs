using DB2Boiler.Infrastructure;
using IBM.Data.Db2;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace DB2Boiler.QueryFactory
{
    public class DB2Query<TResponseModel, TParameterModel> 
        where TResponseModel : DB2ResultMappable, new()
        where TParameterModel : IDB2Parameters, new()
    {
        public string ProcedureName { get; set; }
        public List<string> RequiredParameters { get; set; }
        public List<(string Name, string Value)> ParameterValues { get; set; }
        public HttpRequestData? HttpRequestData { get; set; }
        public IDB2Service? DB2Service { get; set; }
        public ILogger? Logger { get; set; }
        public IDB2Parameters? Parameters {get; set;}

        public DB2Query(string procedureName)
        {
            ProcedureName = procedureName;
            RequiredParameters = new List<string>();
            ParameterValues = new List<(string Name, string Value)>();
            Parameters = new TParameterModel();
        }

        public List<DB2Parameter> GetParameters()
        {
            if (RequiredParameters.Count() == 0)
            {
                return new List<DB2Parameter>();
            }
            if (HttpRequestData == null)
            {
                throw new InvalidOperationException("You need to provide an HttpRequestData by calling UseHttpRequestParameters().");
            }
            if (Parameters == null)
            {
                throw new InvalidOperationException("You need to provide an IDB2Parameters implementation by calling UseIDB2ParametersImplementation<>().");
            }


            return Parameters.MapAndRetrieveParameters(HttpRequestData, ParameterValues);
        }
    }
}
