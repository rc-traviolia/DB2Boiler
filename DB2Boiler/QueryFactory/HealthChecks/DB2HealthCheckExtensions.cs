using Azure;
using DB2Boiler.Infrastructure;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DB2Boiler.QueryFactory.HealthChecks
{
    public static class DB2HealthCheckExtensions
    {
        public static DB2Query<TResponseModel, TParameterModel> UseHttpRequest<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query, HttpRequestData httpRequestData)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            db2Query.HttpRequestData = httpRequestData;
            return db2Query;
        }
        public static DB2Query<TResponseModel, TParameterModel> UseDataService<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query, IDB2Service dataService)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            db2Query.DB2Service = dataService;
            return db2Query;
        }

        public static DB2Query<TResponseModel, TParameterModel> UseLogger<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query, ILogger logger)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            db2Query.Logger = logger;
            return db2Query;
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
