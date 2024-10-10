using DB2Boiler.Infrastructure;
using DB2Boiler.QueryFactory;
using IBM.Data.Db2;
using Microsoft.Azure.Functions.Worker.Http;

namespace DB2Boiler
{
    public interface IDB2Service
    {
        DB2Query<TResponseModel, TParameterModel> CreateDB2Query<TResponseModel, TParameterModel>(string procedureName)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new();

        Task<TResponseModel?> DB2QuerySingle<TResponseModel, TParameterModel>(DB2Query<TResponseModel, TParameterModel> db2Query)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new();

        Task<List<TResponseModel>> DB2QueryMultiple<TResponseModel, TParameterModel>(DB2Query<TResponseModel, TParameterModel> db2Query)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new();

        Task PerformHealthCheck();
    }
}
