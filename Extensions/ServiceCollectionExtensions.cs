using NCRManagementSystem.Configuration;
using NCRManagementSystem.Data;

namespace NCRManagementSystem.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuration objects
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));

            // Database initializer
            services.AddScoped<DatabaseInitializer>();

            // Add other custom services here
            return services;
        }
    }
}