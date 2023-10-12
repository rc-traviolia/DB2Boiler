using DB2Boiler.Infrastructure;
using DB2Boiler.Utilities;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DB2Boiler.QueryFactory
{
    public static class DB2QueryExtensions
    {
        public static DB2Query<TResponseModel, TParameterModel> AddParameterWithValue<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query, string parameterName, string parameterValue)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            db2Query.ParameterValues.Add(new(parameterName, parameterValue));
            return db2Query;
        }
        public static DB2Query<TResponseModel, TParameterModel> RequireParameterFromHttpRequest<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query, string parameterToGuarantee)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            db2Query.RequiredParameters.Add(parameterToGuarantee);
            return db2Query;
        }
        public static DB2Query<TResponseModel, TParameterModel> UseHttpRequestParameters<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query, HttpRequestData httpRequestData)
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

        public static async Task<HttpResponseData> GetSingleResultAsync<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            return await db2Query.GetResultAsync(DB2ResultType.Single);
        }

        public static async Task<HttpResponseData> GetListResultAsync<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        { 
            return await db2Query.GetResultAsync(DB2ResultType.List);
        }

        private static async Task<HttpResponseData> GetResultAsync<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query, DB2ResultType dB2ResultType)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
    {
            if (db2Query.RequiredParameters.Count() > 0 && db2Query.HttpRequestData == null)
            {
                throw new InvalidOperationException("You have required parameters, but no HttpRequestData to pull them from. You need to provide an HttpRequestData by calling UseHttpRequestParameters().");
            }

            if (db2Query.DB2Service == null)
            {
                throw new InvalidOperationException("You must provide an implementation of IDB2Service by calling UseHttpRequestParameters().");
            }

            try
            {
                var missingParameters = new List<string>();
                foreach (var requiredParameter in db2Query.RequiredParameters)
                {
                    if (string.IsNullOrWhiteSpace(db2Query.HttpRequestData?.Query[requiredParameter]?.ToString()))
                    {
                        missingParameters.Add(requiredParameter);
                    }
                }

                if (missingParameters.Count > 0)
                {
                    string reportString = $"Missing Required Parameters: {string.Join(", ", missingParameters.ToArray())}";
                    db2Query.Logger.LogError(new Exception(reportString), reportString);
                    var response = db2Query.HttpRequestData.GuaranteeNotNull().CreateResponse(HttpStatusCode.BadRequest);
                    response.WriteString(reportString);
                    return response;
                }

                if (dB2ResultType == DB2ResultType.List)
                {
                    var result = await db2Query.DB2Service.DB2QueryMultiple<TResponseModel>(db2Query.ProcedureName, db2Query.GetParameters());

                    var response = db2Query.HttpRequestData.GuaranteeNotNull().CreateResponse(HttpStatusCode.OK);
                    await response.WriteAsJsonAsync(result);
                    return response;
                }
                else
                {
                    var result = await db2Query.DB2Service.DB2QuerySingle<TResponseModel>(db2Query.ProcedureName, db2Query.GetParameters());

                    if (result == null)
                    {
                        //return new OkObjectResult(new object());
                        var response = db2Query.HttpRequestData.GuaranteeNotNull().CreateResponse(HttpStatusCode.OK);
                        await response.WriteAsJsonAsync(new object());
                        return response;
                    }
                    else
                    {
                        //return new OkObjectResult(result);
                        var response = db2Query.HttpRequestData.GuaranteeNotNull().CreateResponse(HttpStatusCode.OK);
                        await response.WriteAsJsonAsync(result);
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                var response = db2Query.HttpRequestData.GuaranteeNotNull().CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(ex);
                return response;
            }
        }

    }
}
