using Contracts;
using Entities.ConfigurationModels;
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
using FileService;
using LoggerService;
using Entities.Exceptions;
using Microsoft.Extensions.Azure;
using SmtpClientService;

namespace ChatRoom.Backend.Extensions {
    public static class ServiceExtensions {
        public static void ConfigureCors(this IServiceCollection services) =>
            services.AddCors(options => {
                options.AddPolicy("CorsPolicy", builder =>
                    builder.WithOrigins("http://localhost:4200", "https://april-chatroom-frontend.azurewebsites.net")
                    .AllowAnyMethod() //Change to WithMethods("POST", "GET")
                    .AllowAnyHeader() //Change to WithHeaders("accept", "content-type")
                    .AllowCredentials()
                    .WithExposedHeaders("X-Pagination")); 
            });

        public static void ConfigureLoggerService(this IServiceCollection services) =>
            services.AddSingleton<ILoggerManager, LoggerManager>();

        public static void ConfigureSmtpClientService(this IServiceCollection services) =>
            services.AddSingleton<ISmtpClientManager, SmtpClientManager>();

        public static void ConfigureRedisCache(this IServiceCollection services, IConfiguration configuration) {
            services.AddStackExchangeRedisCache(options => {
                options.Configuration = configuration.GetConnectionString("RedisConnection");
            });

            services.AddSingleton<IRedisCacheManager, RedisCacheManager>();
        }

        public static void ConfigureRepositoryManager(this IServiceCollection services) =>
            services.AddScoped<IRepositoryManager, RepositoryManager>();

        public static void ConfigureServiceManager(this IServiceCollection services) =>
            services.AddScoped<IServiceManager, ServiceManager>();

        public static void ConfigureFileService(this IServiceCollection services) =>
            services.AddSingleton<IFileManager, FileManager>();

        public static void ConfigureDapperConnection(this IServiceCollection services, IConfiguration configuration) =>
            services.AddScoped<IDbConnection>(sp => new SqlConnection(configuration.GetConnectionString("SqlConnection")));

        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration) {
            var jwtSetting = configuration.GetSection("JwtSettings");
            string secretKey = configuration["TOKEN_SECRET_KEY"] ?? throw new JwtSecretKeyNotFoundException();

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
                    options.Events = new JwtBearerEvents {
                        OnMessageReceived = context => {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatRoomHub")) {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
        }

        public static void ConfigureFileUploads(this IServiceCollection services) {
            services.Configure<FormOptions>(o => {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
        }

        public static void ConfigureSignalR(this IServiceCollection services, IConfiguration configuration) {
            var signalRConnection = configuration.GetConnectionString("SignalRConnection");
            if (!string.IsNullOrEmpty(signalRConnection)) {
                services.AddSignalR().AddAzureSignalR(config => {
                    config.ConnectionString = signalRConnection;
                });
            }
            else {
                services.AddSignalR();
            }
        }

        public static void ConfigureAzureBlobStorage(this IServiceCollection services, IConfiguration configuration) {
            services.AddAzureClients(clientBuilder => {
                clientBuilder.AddBlobServiceClient(configuration.GetConnectionString("BlobStorageConnection") ??
                    throw new ConnectionStringNotFoundException("BlobStorageConnection"), preferMsi: true);
            });
        }
    }
}