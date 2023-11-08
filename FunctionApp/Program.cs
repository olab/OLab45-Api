using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OLab.Api.Common;
using OLab.Api.Data;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data;
using OLab.Data.Interface;
using OLab.FunctionApp;
using OLab.FunctionApp.Middleware;
using OLab.FunctionApp.Services;
using System.Net;

var host = new HostBuilder()
    .ConfigureAppConfiguration(builder =>
    {
      builder.AddJsonFile(
        "local.settings.json",
        optional: true,
        reloadOnChange: true);
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

      services.AddOptions<AppSettings>()
        .Configure<IConfiguration>((options, c) =>
        {
          c.GetSection("AppSettings").Bind(options);
        });

      services.AddAzureAppConfiguration();

      services.AddScoped<IUserContext, UserContextService>();

      services.AddSingleton<IOLabLogger, OLabLogger>();
      services.AddSingleton<IOLabConfiguration, OLabConfiguration>();
      services.AddSingleton<IOLabSession, OLabSession>();
      //services.AddScoped<IOLabAuthorization, OLabAuthorization>();
      services.AddScoped<IOLabAuthentication, OLabAuthentication>();
      services.AddSingleton<IUserService, UserService>();
      services.AddSingleton(typeof(IOLabModuleProvider<>), typeof(OLabModuleProvider<>));
      services.AddSingleton<IOLabModuleProvider<IWikiTagModule>, WikiTagProvider>();
      services.AddSingleton<IOLabModuleProvider<IFileStorageModule>, FileStorageProvider>();
    })

    .ConfigureFunctionsWorkerDefaults(builder =>
    {
      builder.UseMiddleware<OLabAuthMiddleware>();
    })

    .ConfigureLogging(builder =>
    {
      builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
    })

    .Build();

host.Run();
