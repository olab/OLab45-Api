using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Api.Model;
using OLab.Api.WikiTag;
using OLab.Azure.Services;
using OLab.Common;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data;
using OLab.Data.Interface;
using System;

namespace OLab.Azure;

public class Program
{
  public static void Main(string[] args)
  {
    var host = new HostBuilder()
        .ConfigureFunctionsWorkerDefaults()   // ? correct isolated worker builder
        .ConfigureAppConfiguration( config =>
        {
          config.AddJsonFile( "host.json", optional: true )
                    .AddJsonFile( "local.settings.json", optional: true );
        } )
        .ConfigureServices( (context, services) =>
        {
          // ? Application Insights goes here for isolated worker
          services.AddApplicationInsightsTelemetryWorkerService();

          var connectionString = context.Configuration.GetConnectionString( "DefaultDatabase" );
          var serverVersion = ServerVersion.AutoDetect( connectionString );

          services.AddDbContext<OLabDBContext>( options =>
                  options.UseMySql( connectionString, serverVersion )
                         .LogTo( Console.WriteLine, LogLevel.None ),
                  ServiceLifetime.Transient );

          services.AddAzureAppConfiguration()
                      .AddSingleton( typeof( IOLabModuleProvider<> ), typeof( OLabModuleProvider<> ) )
                      .AddSingleton<IOLabConfiguration, OLabConfiguration>()
                      .AddSingleton<IOLabLogger, OLabLogger>()
                      .AddSingleton<IOLabModuleProvider<IFileStorageModule>, FileStorageProvider>()
                      .AddSingleton<IOLabModuleProvider<IWikiTagModule>, WikiTagModuleProvider>()
                      .AddTransient<IOLabAuthentication, OLabAuthentication>()
                      .AddTransient<IOLabAuthorization, OLabAuthorization>()
                      .AddTransient<IAuthenticatedContext, AuthenticatedMiddlewareContext>();
        } )
        .Build();

    host.Run();
  }
}
