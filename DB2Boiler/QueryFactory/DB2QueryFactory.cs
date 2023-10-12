using DB2Boiler.Infrastructure;

namespace DB2Boiler.QueryFactory
{
    public static class DB2QueryFactory
    {
        public static DB2Query<TResponseModel, TParameterModel> CreateDB2Query<TResponseModel, TParameterModel>(string procedureName)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            return new DB2Query<TResponseModel, TParameterModel>(procedureName);
        }
    }
}
