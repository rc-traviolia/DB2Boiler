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
        public IDB2Parameters Parameters {get; set;}
        public int Timeout { get; set; }

        public DB2Query(string procedureName)
        {
            ProcedureName = procedureName;
            RequiredParameters = new List<string>();
            ParameterValues = new List<(string Name, string Value)>();
            Parameters = new TParameterModel();
            Timeout = 30;
        }

        public List<DB2Parameter> GetParameters()
        {
            if (RequiredParameters.Count == 0 && ParameterValues.Count == 0)
            {
                return new List<DB2Parameter>();
            }

            return Parameters.MapAndRetrieveParameters(HttpRequestData, ParameterValues);
        }
    }
}
