using DB2Boiler.Infrastructure;
using DB2Boiler.Utilities;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace DB2Boiler.QueryFactory
{
    public static class DB2QueryExtensions
    {
        public static DB2Query<TResponseModel, TParameterModel> AddParameterWithoutValue<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query, string parameterName)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            db2Query.ParameterValues.Add(new(parameterName, ""));
            return db2Query;
        }
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
        /// <summary>
        /// The default is 30 seconds. Using this will override that default.
        /// </summary>
        /// <typeparam name="TResponseModel"></typeparam>
        /// <typeparam name="TParameterModel"></typeparam>
        /// <param name="db2Query"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static DB2Query<TResponseModel, TParameterModel> UseCustomTimeout<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query, int timeoutSeconds)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            db2Query.Timeout = timeoutSeconds;
            return db2Query;
        }

        public static async Task<HttpResponseData> GetSingleHttpResponseDataAsync<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            //code switches based on the optional function being null or not... but the method needs a DB2ResultType to get passed in... so I swapped which
            //parameter is null, even though that return responseModelResponse is not used except to pass in a type.
            return await db2Query.GetHttpResponseDataAsync(DB2ResultType.Single, null, (responseModelResponse) => { return responseModelResponse; });
        }

        public static async Task<HttpResponseData> GetListHttpResponseDataAsync<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            return await db2Query.GetHttpResponseDataAsync(DB2ResultType.List, (responseModelResponse) => { return responseModelResponse; }, null);
        }

        public static async Task<HttpResponseData> GetSingleHttpResponseDataAsync<TResponseModel, TParameterModel, TReplacementResponseModel>(this DB2Query<TResponseModel, TParameterModel> db2Query,
            Func<TResponseModel, TReplacementResponseModel>? optionalFunction = null)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            return await db2Query.GetHttpResponseDataAsync(DB2ResultType.Single, optionalFunction, null);
        }

        public static async Task<HttpResponseData> GetListHttpResponseDataAsync<TResponseModel, TParameterModel, TReplacementResponseModel>(this DB2Query<TResponseModel, TParameterModel> db2Query,
            Func<List<TResponseModel>, TReplacementResponseModel>? optionalFunction = null)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            return await db2Query.GetHttpResponseDataAsync(DB2ResultType.List, null, optionalFunction);
        }

        private static async Task<HttpResponseData> GetHttpResponseDataAsync<TResponseModel, TParameterModel, TReplacementResponseModel>(this DB2Query<TResponseModel, TParameterModel> db2Query, DB2ResultType dB2ResultType,
            Func<TResponseModel, TReplacementResponseModel>? optionalSingleFunction,
            Func<List<TResponseModel>, TReplacementResponseModel>? optionalListFunction)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            if (db2Query.RequiredParameters.Count() > 0 && db2Query.HttpRequestData == null)
            {
                throw new InvalidOperationException("You have required parameters, but no HttpRequestData to pull them from. You need to provide an HttpRequestData by calling UseHttpRequest().");
            }

            if (db2Query.DB2Service == null)
            {
                throw new InvalidOperationException("You must provide an implementation of IDB2Service. You can use the default one by calling AddDB2Service() in Program.cs.");
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
                    var response = db2Query.HttpRequestData!.CreateResponse(HttpStatusCode.BadRequest);
                    response.WriteString(reportString);
                    return response;
                }

                if (dB2ResultType == DB2ResultType.List)
                {
                    if (optionalListFunction == null)
                    {
                        var result = await db2Query.DB2Service.DB2QueryMultiple(db2Query);
                        var response = db2Query.HttpRequestData!.CreateResponse(HttpStatusCode.OK);
                        await response.WriteAsJsonAsync(result);
                        return response;
                    }
                    else
                    {
                        var initialResult = await db2Query.DB2Service.DB2QueryMultiple(db2Query);
                        var result = optionalListFunction(initialResult);
                        var response = db2Query.HttpRequestData!.CreateResponse(HttpStatusCode.OK);
                        await response.WriteAsJsonAsync(result);
                        return response;
                    }
                }
                else
                {
                    var initialResult = await db2Query.DB2Service.DB2QuerySingle(db2Query);
                    if (optionalSingleFunction == null)
                    {
                        if (initialResult == null)
                        {
                            //return new OkObjectResult(new object());
                            var response = db2Query.HttpRequestData!.CreateResponse(HttpStatusCode.OK);
                            await response.WriteAsJsonAsync(new object());
                            return response;
                        }
                        else
                        {
                            var response = db2Query.HttpRequestData!.CreateResponse(HttpStatusCode.OK);
                            await response.WriteAsJsonAsync(initialResult);
                            return response;
                        }
                    }
                    else
                    {
                        var result = optionalSingleFunction(initialResult);
                        var response = db2Query.HttpRequestData!.CreateResponse(HttpStatusCode.OK);
                        await response.WriteAsJsonAsync(result);
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                var response = db2Query.HttpRequestData!.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(ex);
                return response;
            }
        }
    }
}
