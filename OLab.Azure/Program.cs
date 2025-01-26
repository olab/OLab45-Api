using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OLab.Access.Interfaces;
using OLab.Access;
using OLab.Api.Common;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Api.WikiTag;
using OLab.Azure.Middleware;
using OLab.Azure.Services;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Interface;
using OLab.Data;
using DocumentFormat.OpenXml.Wordprocessing;

internal class Program
{
  private static void Main(string[] args)
  {
    var host = new HostBuilder();

    host
      .ConfigureFunctionsWebApplication()
      .ConfigureLogging( logging =>
      {
        /*
          * By default, logs with LogLevel.Warning or higher are sent to Application Insights.
          * To change this, remove the default rule so other log levels are sent to Application Insights.
          * See for more information: https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=hostbuilder%2Cwindows#managing-log-levels
          * The default log level for Azure Functions is Information. So by removing the default rule, Information and above will be sent to Application Insights.
          * 
          * For configuring the loglevel per function. See the following documentation: https://learn.microsoft.com/en-us/azure/azure-functions/configure-monitoring?tabs=v2#configure-categories
          * In example: 
          * "logLevel": {
                "Host.Aggregator": "Trace", // Default
                "Host.Results": "Information", // Default
                "Function": "Information", // Default. Entries related to running a function are assigned a category of Function.<FUNCTION_NAME>.
                "Function.Function1.User": "Warning", 
                "Function.Function2.User": "Error",
                "Function.GetUsers": "Information", // Entries related to running a function are assigned a category of Function.<FUNCTION_NAME>.
                "Function.GetUsers.User": "Information", // Entries created by user code inside the function, such as when calling logger.LogInformation(), are assigned a category of Function.<FUNCTION_NAME>.User.
            },
        */
        logging.Services.Configure<LoggerFilterOptions>( options =>
        {
          LoggerFilterRule defaultRule = options.Rules.FirstOrDefault( rule => rule.ProviderName
            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider" );

          if ( defaultRule is not null )
          {
            options.Rules.Remove( defaultRule );
          }
        } );

        // Disable IHttpClientFactory Informational logs. Source: https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=hostbuilder%2Cwindows#logging
        // Note -- you can also remove the handler that does the logging: https://github.com/aspnet/HttpClientFactory/issues/196#issuecomment-432755765 
        logging.AddFilter( "System.Net.Http.HttpClient", LogLevel.Warning );
      } )
      .ConfigureAppConfiguration( (hostContext, config) =>
      {
        // Add appsettings.json and appsettings.{environment}.json configuration so we can set configuration in source control and add configuration per environment.
        // Add in example a file called appsettings.json to the root and set the properties to:
        // Build Action: Content
        // Copy to Output Directory: Copy if newer
        //
        // Content:
        // {
        //    "Key1": "Value A",
        //    "KeysNested": {
        //        "Key2": "Value B",
        //        "Key3": "Value C"
        //    }
        //}

        // When this sample project is hosted on Linux-x64. The file appsettings.json is not loaded without this hack on Linux.
        // This hack only works on Linux. For dedicated app service plan and consumption plan. See: https://stackoverflow.com/a/79178062/801005
        if ( hostContext.HostingEnvironment.IsDevelopment() == false )
          config.SetBasePath( "/home/site/wwwroot" );

        // Add configuration from appsettings.json and appsettings.{Environment}.json
        config
          //.SetBasePath( Directory.GetCurrentDirectory() ) // Remove this line when running on Linux consumption plan. See above and https://stackoverflow.com/a/79178062/801005
          //.AddJsonFile( "appsettings.json", optional: true, reloadOnChange: true )
          //.AddJsonFile( $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true )
          .AddEnvironmentVariables()
          .Build();

        // Add local.settings.json and user secrets in development environment
        if ( hostContext.HostingEnvironment.IsDevelopment() )
        {
          config.AddJsonFile( "local.settings.json" );
          config.AddUserSecrets<Program>( true );
        }

        config.AddJsonFile( "host.json", optional: true );
      } )

      .ConfigureFunctionsWorkerDefaults( worker =>
      {
        worker.UseMiddleware<BootstrapMiddleware>();
        worker.UseWhen<OLabAuthMiddleware>( OLabAuthMiddleware.CanInvoke );
      } )

      .ConfigureLogging( (hostingContext, logging) =>
      {
        logging.AddConfiguration( hostingContext.Configuration.GetSection( "Logging" ) );
      } )

      .ConfigureServices( (hostingContext, services ) =>
      {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Setup DI
        services.AddScoped<IUserContext, FunctionAppUserContext>();
        services.AddSingleton<IOLabLogger, OLabLogger>();
        services.AddSingleton<IOLabConfiguration, OLabConfiguration>();
        services.AddScoped<IOLabAuthentication, OLabAuthentication>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton( typeof( IOLabModuleProvider<> ), typeof( OLabModuleProvider<> ) );
        services.AddSingleton<IOLabModuleProvider<IWikiTagModule>, WikiTagModuleProvider>();
        services.AddSingleton<IOLabModuleProvider<IFileStorageModule>, FileStorageProvider>();

        var connectionString = hostingContext.Configuration.GetConnectionString( "DefaultDatabase" );
        var serverVersion = ServerVersion.AutoDetect( connectionString );

        services.AddDbContext<OLabDBContext>( options =>
            options.UseMySql( connectionString, serverVersion )
                .LogTo( Console.WriteLine, LogLevel.Error )
                //.EnableSensitiveDataLogging()
                //.EnableDetailedErrors()
                );
      } );

    host.Build().Run();
  }

  private static void Main2(string[] args)
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

    //builder.Logging.Services.Configure<LoggerFilterOptions>( options =>
    //{
    //  // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning
    //  // and more severe logs. Application Insights requires an explicit override.
    //  // Log levels can also be configured using appsettings.json. For more information,
    //  // see https://learn.microsoft.com/azure/azure-monitor/app/worker-service#ilogger-logs
    //  var defaultRule = options.Rules.FirstOrDefault( rule => rule.ProviderName
    //      == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider" );
    //  if ( defaultRule is not null )
    //    options.Rules.Remove( defaultRule );
    //} );

    builder.Services.AddAzureAppConfiguration();

    builder.Services.AddScoped<IUserContext, FunctionAppUserContext>();
    builder.Services.AddSingleton<IOLabLogger, OLabLogger>();
    builder.Services.AddSingleton<IOLabConfiguration, OLabConfiguration>();
    builder.Services.AddScoped<IOLabAuthentication, OLabAuthentication>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddSingleton( typeof( IOLabModuleProvider<> ), typeof( OLabModuleProvider<> ) );
    builder.Services.AddSingleton<IOLabModuleProvider<IWikiTagModule>, WikiTagModuleProvider>();
    builder.Services.AddSingleton<IOLabModuleProvider<IFileStorageModule>, FileStorageProvider>();

    builder.UseMiddleware<BootstrapMiddleware>();
    builder.UseWhen<OLabAuthMiddleware>( OLabAuthMiddleware.CanInvoke );
    //builder.UseWhen<OpenAuthMiddleware>( OpenAuthMiddleware.CanInvoke );

    builder.Build().Run();
  }
}