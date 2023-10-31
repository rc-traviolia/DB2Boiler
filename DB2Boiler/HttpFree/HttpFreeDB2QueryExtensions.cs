using DB2Boiler.Infrastructure;
using DB2Boiler.QueryFactory;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DB2Boiler.HttpFree
{
    public static class HttpFreeDB2QueryExtensions
    {

        private static TResponseModel DefaultReplacementFunction<TResponseModel>(TResponseModel responseModel)
            where TResponseModel: DB2ResultMappable, new()
        {
            return responseModel;
        }

        public static async Task<TReplacementResponseModel> GetSingleResponseModelAsync<TResponseModel, TParameterModel, TReplacementResponseModel>(this DB2Query<TResponseModel, TParameterModel> db2Query,
            Func<TResponseModel, TReplacementResponseModel> optionalFunction )
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
            where TReplacementResponseModel : class, new()
        {
            var initialSingleResult = await db2Query.GetSingleResponseModelAsync();
            if (initialSingleResult == null)
            {
                return null;
            }
            else
            {
                return optionalFunction(initialSingleResult);
            }
        }
        public static async Task<List<TReplacementResponseModel>> GetListResponseModelAsync<TResponseModel, TParameterModel, TReplacementResponseModel>(this DB2Query<TResponseModel, TParameterModel> db2Query,
            Func<TResponseModel, List<TReplacementResponseModel>> optionalFunction)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
            where TReplacementResponseModel : class, new()
        {
            var initialSingleResult = await db2Query.GetSingleResponseModelAsync();
            if (initialSingleResult == null)
            {
                return new List<TReplacementResponseModel>();
            }
            else
            {
                return optionalFunction(initialSingleResult);
            }
        }
        public static async Task<TReplacementResponseModel> GetSingleResponseModelAsync<TResponseModel, TParameterModel, TReplacementResponseModel>(this DB2Query<TResponseModel, TParameterModel> db2Query,
           Func<List<TResponseModel>, TReplacementResponseModel> optionalFunction)
           where TResponseModel : DB2ResultMappable, new()
           where TParameterModel : IDB2Parameters, new()
            where TReplacementResponseModel : class, new()
        {
            var initialListResult = await db2Query.GetListResponseModelAsync();
            return optionalFunction(initialListResult);
        }
        public static async Task<List<TReplacementResponseModel>> GetListResponseModelAsync<TResponseModel, TParameterModel, TReplacementResponseModel>(this DB2Query<TResponseModel, TParameterModel> db2Query,
            Func<List<TResponseModel>, List<TReplacementResponseModel>> optionalFunction)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
            where TReplacementResponseModel : class, new()
        {
            var initialResult = await db2Query.GetListResponseModelAsync();
            return optionalFunction(initialResult);
        }
        public static async Task<TResponseModel?> GetSingleResponseModelAsync<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
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
                    throw new InvalidOperationException($"Missing Required Parameters: {string.Join(", ", missingParameters.ToArray())}");
                }

                //Get data
                return await db2Query.DB2Service.DB2QuerySingle(db2Query);

            }
            catch (Exception ex)
            {
                db2Query.Logger.LogError(ex.Message, ex);
                return null;
            }
        }
        public static async Task<List<TResponseModel>> GetListResponseModelAsync<TResponseModel, TParameterModel>(this DB2Query<TResponseModel, TParameterModel> db2Query)
           where TResponseModel : DB2ResultMappable, new()
           where TParameterModel : IDB2Parameters, new()
        {
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
                    throw new InvalidOperationException($"Missing Required Parameters: {string.Join(", ", missingParameters.ToArray())}");
                }

                //Get data
                return await db2Query.DB2Service.DB2QueryMultiple(db2Query);
                    
               
            }
            catch (Exception ex)
            {
                db2Query.Logger.LogError(ex.Message, ex);
                return new List<TResponseModel>();
            }
        }
    }
}
