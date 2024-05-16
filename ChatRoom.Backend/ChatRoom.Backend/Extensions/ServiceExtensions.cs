using Contracts;
using LoggerService;
using Microsoft.Data.SqlClient;
using Repository;
using Service.Contracts;
using System.Data;

namespace ChatRoom.Backend.Extensions {
    public static class ServiceExtensions {
        public static void ConfigureCors(this IServiceCollection services) =>
            services.AddCors(options => {
                options.AddPolicy("CorsPolicy", builder =>
                    builder.AllowAnyOrigin() //Change to WithOrigins("https://example.com")
                    .AllowAnyMethod() //Change to WithMethods("POST", "GET")
                    .AllowAnyHeader()); //Change to WithHeaders("accept", "content-type")
            });

        public static void ConfigureLoggerService(this IServiceCollection services) =>
            services.AddSingleton<ILoggerManager, LoggerManager>();

        public static void ConfigureRepositoryManager(this IServiceCollection services) =>
            services.AddScoped<IRepositoryManager, RepositoryManager>();

        public static void ConfigureServiceManager(this IServiceCollection services) =>
            services.AddScoped<IServiceManager, IServiceManager>();

        public static void ConfigureDapperConnection(this IServiceCollection services, IConfiguration configuration) =>
            services.AddScoped<IDbConnection>(sp => new SqlConnection(configuration.GetConnectionString("SqlConnection")));
    }
}