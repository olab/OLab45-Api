using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Api.Common;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Api.WikiTag;
using OLab.Azure.Middleware;
using OLab.Azure.Services;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data;
using OLab.Data.Interface;
using System;

var builder = FunctionsApplication.CreateBuilder( args );
builder.ConfigureFunctionsWebApplication();

builder.Configuration
  .AddEnvironmentVariables()
  .AddJsonFile( "host.json", optional: true )
  .AddJsonFile( "local.settings.json", optional: true );

var connectionString = builder.Configuration.GetConnectionString( "DefaultDatabase" );
var serverVersion = ServerVersion.AutoDetect( connectionString );

builder.Services.AddApplicationInsightsTelemetryWorkerService();

builder.Services
  .ConfigureFunctionsApplicationInsights()
  .AddDbContext<OLabDBContext>( options =>
            options.UseMySql( connectionString, serverVersion )
                .LogTo( Console.WriteLine, LogLevel.None ),
                //.EnableSensitiveDataLogging()
                //.EnableDetailedErrors()
                ServiceLifetime.Transient );

builder.Services.AddOptions<AppSettings>()
  .Configure<IConfiguration>( (options, c) =>
  {
    c.GetSection( "AppSettings" ).Bind( options );
  } );

builder.Services
  .AddAzureAppConfiguration()
  .AddSingleton( typeof( IOLabModuleProvider<> ), typeof( OLabModuleProvider<> ) )
  .AddSingleton<IOLabConfiguration, OLabConfiguration>()
  .AddSingleton<IOLabLogger, OLabLogger>()
  .AddSingleton<IOLabModuleProvider<IFileStorageModule>, FileStorageProvider>()
  .AddSingleton<IOLabModuleProvider<IWikiTagModule>, WikiTagModuleProvider>()
  .AddTransient<IOLabAuthentication, OLabAuthentication>()
  .AddTransient<IUserContext, OLabAuthMiddlewareContext>()
  .AddTransient<IUserService, UserService>();
builder.UseMiddleware<BootstrapMiddleware>();
builder.UseWhen<OLabAuthMiddleware>( OLabAuthMiddleware.CanInvoke );

var host = builder.Build();

host.Run();
