using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DB2Boiler.QueryFactory.HealthChecks
{
    public class DB2HealthCheck
    {
        public HttpRequestData? HttpRequestData { get; set; }
        public IDB2Service? DB2Service { get; set; }
        public ILogger? Logger { get; set; }
    }

}
