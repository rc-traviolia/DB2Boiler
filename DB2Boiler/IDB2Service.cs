using DB2Boiler.Infrastructure;
using IBM.Data.Db2;

namespace DB2Boiler
{
    public interface IDB2Service
    {
        Task<TResponseModel?> DB2QuerySingle<TResponseModel>(string procedureName, List<DB2Parameter> parameterList) where TResponseModel : DB2ResultMappable, new();
        Task<List<TResponseModel>> DB2QueryMultiple<TResponseModel>(string procedureName, List<DB2Parameter> parameterList) where TResponseModel : DB2ResultMappable, new();
    }
}
