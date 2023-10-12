using DB2Boiler.Infrastructure;
using DB2Boiler.Utilities;
using Dot.Services.DB2.LoadsToLoaded.Configuration;
using IBM.Data.Db2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Linq;

namespace DB2Boiler
{
    public class DB2Service : IDB2Service
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;

        public DB2Service(IOptionsSnapshot<AppSettings> appSettings, ILoggerFactory loggerFactory)
        {
            _appSettings = appSettings.Value;
            _logger = loggerFactory.CreateLogger<DB2Service>();

        }

        public async Task<TResponseModel?> DB2QuerySingle<TResponseModel>(string procedureName, List<DB2Parameter> parameterList) where TResponseModel : DB2ResultMappable, new()
        {
            var result = await DB2QueryMultiple<TResponseModel>(procedureName, parameterList);
            return result.FirstOrDefault();

        }

        public async Task<List<TResponseModel>> DB2QueryMultiple<TResponseModel>(string procedureName, List<DB2Parameter> parameterList) where TResponseModel : DB2ResultMappable, new()
        {
            if(parameterList == null)
            {
                throw new Exception("DB2QueryMultiple was called with null parameterList. You must provide some object reference, even if it has no p");
            }

            var commandText = $"{_appSettings.LibraryName}.{procedureName}";
            var storedProcResults = new List<TResponseModel>();
            var outParametersPresent = false;

            try
            {
                using (var connection = new DB2Connection(_appSettings.IBMiConnectionString))
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
                _logger.LogError(ex, $"Exception thrown while executing DB2QUery {_appSettings.LibraryName}.{procedureName}");
            }

            return storedProcResults;
        }
    }
}
