using Contracts;
using Entities.ConfigurationModels;
using LoggerService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using RedisCacheService;
using Repository;
using Service;
using Service.Contracts;
using System.Data;
using System.Text;

namespace ChatRoom.Backend.Extensions {
    public static class ServiceExtensions {
        public static void ConfigureCors(this IServiceCollection services) =>
            services.AddCors(options => {
                options.AddPolicy("CorsPolicy", builder =>
                    builder.AllowAnyOrigin() //Change to WithOrigins("https://example.com")
                    .AllowAnyMethod() //Change to WithMethods("POST", "GET")
                    .AllowAnyHeader() //Change to WithHeaders("accept", "content-type")
                    .WithExposedHeaders("X-Pagination")); 
            });

        public static void ConfigureLoggerService(this IServiceCollection services) =>
            services.AddSingleton<ILoggerManager, LoggerManager>();



        public static void ConfigureRedisCacheService(this IServiceCollection services) =>
            services.AddSingleton<IRedisCacheManager, RedisCacheManager>();

        public static void ConfigureRepositoryManager(this IServiceCollection services) =>
            services.AddScoped<IRepositoryManager, RepositoryManager>();

        public static void ConfigureServiceManager(this IServiceCollection services) =>
            services.AddScoped<IServiceManager, ServiceManager>();

        public static void ConfigureDapperConnection(this IServiceCollection services, IConfiguration configuration) =>
            services.AddScoped<IDbConnection>(sp => new SqlConnection(configuration.GetConnectionString("SqlConnection")));

        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration) {
            var jwtSetting = configuration.GetSection("JwtSettings");
            string secretKey = jwtSetting["secretKey"] ?? string.Empty;

            services.AddAuthentication(opt => {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSetting["validIssuer"],
                        ValidAudience = jwtSetting["validAudience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                    };
                });
        }
        
        public static void ConfigureSmtpCredentials(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration
            .GetSection("SMTPConfiguration")
            .Get<EmailConfiguration>();
            services.AddSingleton(config!);
        }

        public static void ConfigureFileUploads(this IServiceCollection services) {
            services.Configure<FormOptions>(o => {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
        }
    }
}