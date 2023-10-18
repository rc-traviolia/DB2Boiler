using DB2Boiler.Configuration;
using DB2Boiler.Infrastructure;
using DB2Boiler.Utilities;
using IBM.Data.Db2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

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

        public async Task<TResponseModel?> DB2QuerySingle<TResponseModel>(string procedureName, List<DB2Parameter> parameterList) where TResponseModel : DB2ResultMappable, new()
        {
            var result = await DB2QueryMultiple<TResponseModel>(procedureName, parameterList);
            return result.FirstOrDefault();

        }

        public async Task<List<TResponseModel>> DB2QueryMultiple<TResponseModel>(string procedureName, List<DB2Parameter> parameterList) where TResponseModel : DB2ResultMappable, new()
        {
            if (parameterList == null)
            {
                throw new Exception("DB2QueryMultiple was called with null parameterList. You must provide some object reference, even if it has no p");
            }

            var commandText = $"{_settings.LibraryName}.{procedureName}";
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
                        foreach (var parameter in parameterList)
                        {
                            command.Parameters.Add(parameter);
                            if (parameter.Direction != ParameterDirection.Input)
                            {
                                outParametersPresent = true;
                            }
                        }

                        _logger.LogInformation($"Submitting {commandText} with Parameters:\n{parameterList.ToIndentedJson()}");

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
                _logger.LogError(ex, $"Exception thrown while executing DB2QUery {_settings.LibraryName}.{procedureName}");
            }

            return storedProcResults;
        }
    }
}
