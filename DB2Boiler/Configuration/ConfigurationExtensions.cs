using Microsoft.Extensions.DependencyInjection;

namespace DB2Boiler.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddDB2Service(this IServiceCollection services)
        {
            return services.AddSingleton<IDB2Service, DB2Service>();
        }
    }
}
