using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using OLab.Api.Utils;
using OLab.Access.Interfaces;
using OLab.Access;
using OLab.Api.Common;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Interface;
using OLab.Data;
using OLab.FunctionApp.Middleware;
using OLab.FunctionApp.Services;
using OLab.TurkTalk.Data.Models;
using OLab.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.Interface;

namespace OLab.FunctionApp
{
  public class Program
  {
    public static void Main()
    {
      var host = new HostBuilder()
      .ConfigureAppConfiguration(builder =>
      {
        builder.AddJsonFile(
          "local.settings.json",
          optional: true,
          reloadOnChange: true);
      })

      .ConfigureLogging(builder =>
      {
        builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
      })

      .ConfigureServices((context, services) =>
      {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
#if DEBUG
          Formatting = Formatting.Indented,
#endif
          ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        var connectionString = Environment.GetEnvironmentVariable("DefaultDatabase");
        var serverVersion = ServerVersion.AutoDetect(connectionString);

        services.AddDbContext<OLabDBContext>(options =>
          options.UseMySql(connectionString, serverVersion)
            .EnableDetailedErrors(), ServiceLifetime.Scoped);
        //.AddLogging(options => options.SetMinimumLevel(LogLevel.Information));

        services.AddDbContext<TTalkDBContext>(options =>
          options.UseMySql(connectionString, serverVersion)
            .EnableDetailedErrors(), ServiceLifetime.Scoped);

        services.AddOptions<AppSettings>()
          .Configure<IConfiguration>((options, c) =>
          {
            c.GetSection("AppSettings").Bind(options);
          });

        services.AddAzureAppConfiguration();

        services.AddScoped<IUserContext, UserContextService>();
        services.AddSingleton<IOLabLogger, OLabLogger>();
        services.AddSingleton<IOLabConfiguration, OLabConfiguration>();
        services.AddScoped<IOLabAuthentication, OLabAuthentication>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton(typeof(IOLabModuleProvider<>), typeof(OLabModuleProvider<>));
        services.AddSingleton<IOLabModuleProvider<IWikiTagModule>, WikiTagProvider>();
        services.AddSingleton<IOLabModuleProvider<IFileStorageModule>, FileStorageProvider>();

        services.AddSingleton<IConference, Conference>();

      })

      .ConfigureFunctionsWorkerDefaults(builder =>
      {
        builder.UseMiddleware<BootstrapMiddleware>();
        builder.UseWhen<OLabAuthMiddleware>(context => OLabAuthMiddleware.CanInvoke(context));
        builder.UseWhen<OpenAuthMiddleware>(context => OpenAuthMiddleware.CanInvoke(context));
        builder.UseWhen<TTalkAuthMiddleware>(context => TTalkAuthMiddleware.CanInvoke(context));
      })

      .Build();

      host.Run();
    }
  }
}