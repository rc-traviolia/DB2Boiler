using IBM.Data.Db2;
using Microsoft.Azure.Functions.Worker.Http;

namespace DB2Boiler.Infrastructure
{
    public interface IDB2Parameters
    {
        List<DB2Parameter> MapAndRetrieveParameters(HttpRequestData httpRequestData, List<(string Name, string Value)> parameterValues);
    }
}
