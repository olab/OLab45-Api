using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OLab.Access.Interfaces;
using OLab.Access;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Api.WikiTag;
using OLab.Azure.Services;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Interface;
using OLab.Data;
using OLab.Azure.Middleware;
using OLab.Common.Contracts;
using OLab.Common;
using OLab.Api.Utils;
using OLab.Api.Common;

internal class Program
{
  private static void Main(string[] args)
  {
    var builder = FunctionsApplication.CreateBuilder( args );

    builder.ConfigureFunctionsWebApplication();

    var connectionString = builder.Configuration.GetConnectionString( "DefaultDatabase" );
    var serverVersion = ServerVersion.AutoDetect( connectionString );

    // Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
    builder.Services
      .AddApplicationInsightsTelemetryWorkerService()
      .ConfigureFunctionsApplicationInsights()
      .AddDbContext<OLabDBContext>( options =>
            options.UseMySql( connectionString, serverVersion )
                .LogTo( Console.WriteLine, LogLevel.Error )
                //.EnableSensitiveDataLogging()
                //.EnableDetailedErrors()
                );

    builder.Services.AddOptions<AppSettings>()
      .Configure<IConfiguration>( (options, c) =>
      {
        c.GetSection( "AppSettings" ).Bind( options );
      } );

    builder.Logging.Services.Configure<LoggerFilterOptions>( options =>
    {
      // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning
      // and more severe logs. Application Insights requires an explicit override.
      // Log levels can also be configured using appsettings.json. For more information,
      // see https://learn.microsoft.com/azure/azure-monitor/app/worker-service#ilogger-logs
      var defaultRule = options.Rules.FirstOrDefault( rule => rule.ProviderName
          == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider" );
      if ( defaultRule is not null )
        options.Rules.Remove( defaultRule );
    } );

    builder.Services.AddAzureAppConfiguration();

    builder.Services.AddScoped<IUserContext, FunctionUserContextService>();
    builder.Services.AddSingleton<IOLabLogger, OLabLogger>();
    builder.Services.AddSingleton<IOLabConfiguration, OLabConfiguration>();
    builder.Services.AddScoped<IOLabAuthentication, OLabAuthentication>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddSingleton( typeof( IOLabModuleProvider<> ), typeof( OLabModuleProvider<> ) );
    builder.Services.AddSingleton<IOLabModuleProvider<IWikiTagModule>, WikiTagModuleProvider>();
    builder.Services.AddSingleton<IOLabModuleProvider<IFileStorageModule>, FileStorageProvider>();

    builder.UseMiddleware<BootstrapMiddleware>();
    //builder.UseWhen<OLabAuthMiddleware>( OLabAuthMiddleware.CanInvoke );
    //builder.UseWhen<OpenAuthMiddleware>( OpenAuthMiddleware.CanInvoke );

    builder.Build().Run();
  }
}