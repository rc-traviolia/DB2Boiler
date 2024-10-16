﻿using DB2Boiler.Infrastructure;
using DB2Boiler.QueryFactory.HealthChecks;
using IBM.Data.Db2;
using Microsoft.Azure.Functions.Worker.Http;

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

        public static DB2HealthCheck CreateDB2HealthCheck()
        {
            return new DB2HealthCheck();
        }
    }
}
