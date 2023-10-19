using DB2Boiler.Infrastructure;
using DB2Boiler.QueryFactory;
using IBM.Data.Db2;

namespace DB2Boiler
{
    public interface IDB2Service
    {
        Task<TResponseModel?> DB2QuerySingle<TResponseModel, TParameterModel>(DB2Query<TResponseModel, TParameterModel> db2Query)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new();

        Task<List<TResponseModel>> DB2QueryMultiple<TResponseModel, TParameterModel>(DB2Query<TResponseModel, TParameterModel> db2Query)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new();
    }
}
