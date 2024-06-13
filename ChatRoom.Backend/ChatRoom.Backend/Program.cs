using ChatRoom.Backend;
using ChatRoom.Backend.Extensions;
using ChatRoom.Backend.Presentation.ActionFilters;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Microsoft.Extensions.Azure;
using ChatRoom.Backend.Presentation.Hubs;

var builder = WebApplication.CreateBuilder(args);

LogManager.Setup().LoadConfigurationFromFile(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

builder.Services.ConfigureCors();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureDapperConnection(builder.Configuration);
builder.Services.ConfigureSmtpCredentials(builder.Configuration);
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.Configure<ApiBehaviorOptions>(options => {
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.ConfigureFileUploads();
builder.Services.AddSignalR();

builder.Services.AddControllers(config => {
    config.RespectBrowserAcceptHeader = true;
    config.ReturnHttpNotAcceptable = true;
})
.AddApplicationPart(typeof(ChatRoom.Backend.Presentation.AssemblyReference).Assembly);
builder.Services.AddAzureClients(clientBuilder => {
    clientBuilder.AddBlobServiceClient(builder.Configuration["ChatroomLocalStorage:blob"]!, preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["ChatroomLocalStorage:queue"]!, preferMsi: true);
});

var app = builder.Build();

app.UseExceptionHandler(opt => { });

// Configure the HTTP request pipeline.
if(app.Environment.IsProduction())
    app.UseHsts();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseForwardedHeaders(new ForwardedHeadersOptions {
    ForwardedHeaders = ForwardedHeaders.All
});

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatRoomHub>("/chatRoomHub");

app.Run();
