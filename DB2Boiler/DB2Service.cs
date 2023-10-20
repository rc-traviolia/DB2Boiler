using DB2Boiler.Configuration;
using DB2Boiler.Infrastructure;
using DB2Boiler.QueryFactory;
using DB2Boiler.Utilities;
using IBM.Data.Db2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace DB2Boiler
{
    public class DB2Service : IDB2Service
    {
        private readonly DB2Settings _settings;
        private readonly ILogger _logger;

        public DB2Service(IOptionsSnapshot<DB2Settings> settings, ILoggerFactory loggerFactory)
        {
            _settings = settings.Value;
            _logger = loggerFactory.CreateLogger<DB2Service>();

            if (string.IsNullOrWhiteSpace(_settings.LibraryName))
            {
                throw new InvalidOperationException("You must have a LibraryName in your configuration for DB2Settings");
            }
            if (string.IsNullOrWhiteSpace(_settings.ConnectionString))
            {
                throw new InvalidOperationException("You must have a ConnectionString in your configuration for DB2Settings");
            }

        }
        public DB2Query<TResponseModel, TParameterModel> CreateDB2Query<TResponseModel, TParameterModel>(string procedureName)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            return new DB2Query<TResponseModel, TParameterModel>(procedureName).UseDataService(this).UseLogger(_logger);
        }

        public async Task<TResponseModel?> DB2QuerySingle<TResponseModel, TParameterModel>(DB2Query<TResponseModel, TParameterModel> db2Query)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            var result = await DB2QueryMultiple(db2Query);
            return result.FirstOrDefault();

        }

        
        public async Task<List<TResponseModel>> DB2QueryMultiple<TResponseModel, TParameterModel>(DB2Query<TResponseModel, TParameterModel> db2Query)
            where TResponseModel : DB2ResultMappable, new()
            where TParameterModel : IDB2Parameters, new()
        {
            var parameterList = db2Query.GetParameters();
            if (parameterList == null)
            {
                throw new Exception("DB2QueryMultiple was called with null parameterList. You must provide some object reference, even if this query doesn't require parameters.");
            }

            var commandText = $"{_settings.LibraryName}.{db2Query.ProcedureName}";
            var storedProcResults = new List<TResponseModel>();
            var outParametersPresent = false;

            try
            {
                using (var connection = new DB2Connection(_settings.ConnectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        await connection.OpenAsync();

                        command.CommandText = commandText;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = db2Query.Timeout;

                        foreach (var parameter in parameterList)
                        {
                            command.Parameters.Add(parameter);
                            if (parameter.Direction != ParameterDirection.Input)
                            {
                                outParametersPresent = true;
                            }
                        }

                        _logger.LogInformation($"Submitting {commandText} with Parameters:\n{parameterList.ToJsonParameters()}");

                        if (!outParametersPresent)
                        {

                            var reader = await command.ExecuteReaderAsync();

                            while (await reader.ReadAsync())
                            {
                                var newRecord = new TResponseModel();
                                newRecord.GetDataFromReader(reader);
                                newRecord.GetDataFromOutputParameters(command);
                                storedProcResults.Add(newRecord);
                            }

                            await reader.CloseAsync();
                        }
                        else
                        {
                            var reader = await command.ExecuteReaderAsync();

                            var newRecord = new TResponseModel();
                            newRecord.GetDataFromOutputParameters(command);
                            storedProcResults.Add(newRecord);

                            await reader.CloseAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception thrown while executing DB2Query {_settings.LibraryName}.{db2Query.ProcedureName}");
            }

            return storedProcResults;
        }
    }
}
