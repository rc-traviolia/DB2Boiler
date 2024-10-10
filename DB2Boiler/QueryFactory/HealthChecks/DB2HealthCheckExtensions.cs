using Azure;
using DB2Boiler.Infrastructure;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DB2Boiler.QueryFactory.HealthChecks
{
    public static class DB2HealthCheckExtensions
    {
        public static DB2HealthCheck UseHttpRequest(this DB2HealthCheck db2HealthCheck, HttpRequestData httpRequestData)
        {
            db2HealthCheck.HttpRequestData = httpRequestData;
            return db2HealthCheck;
        }
        public static DB2HealthCheck UseDataService(this DB2HealthCheck db2HealthCheck, IDB2Service dataService)
        {
            db2HealthCheck.DB2Service = dataService;
            return db2HealthCheck;
        }

        public static DB2HealthCheck UseLogger(this DB2HealthCheck db2HealthCheck, ILogger logger)
        {
            db2HealthCheck.Logger = logger;
            return db2HealthCheck;
        }

        private static async Task<HttpResponseData> GetHttpResponseData(this DB2HealthCheck db2HealthCheck)
        {
            try
            {
                await db2HealthCheck.DB2Service!.PerformHealthCheck();
                return db2HealthCheck.HttpRequestData!.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                if(db2HealthCheck.Logger != null)
                {
                    db2HealthCheck.Logger.LogError(ex, ex.Message);
                }

                var response = db2HealthCheck.HttpRequestData!.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ex);
                return response;
            }
        }
    }

}
