using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OLab.Api.FunctionApp;
using OLab.Api.FunctionApp.Functions;
using OLab.Api.FunctionApp.Middleware;
using OLab.Api.FunctionApp.Services;
using OLab.Api.Data;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Api.Utils;

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
      services.AddTransient<IUserContext, FunctionAppUserContext>();
      services.AddScoped<IUserService, OLabUserService>();
      services.AddScoped<IOLabSession, OLabSession>();

      var connectionString = Environment.GetEnvironmentVariable("DefaultDatabase");
      var serverVersion = ServerVersion.AutoDetect(connectionString);
      services.AddDbContext<OLabDBContext>(options =>
        options.UseMySql(connectionString, serverVersion)
          .EnableDetailedErrors())
          .AddLogging(options => options.SetMinimumLevel(LogLevel.Warning));

      services.AddOptions<AppSettings>()
        .Configure<IConfiguration>((options, c) =>
        {
          c.GetSection("AppSettings").Bind(options);
        });

      services.AddAzureAppConfiguration();

    })

    .ConfigureFunctionsWorkerDefaults(builder =>
    {
      builder.UseMiddleware<OLabAuthMiddleware>();
      //builder.UseMiddleware<ExceptionLoggingMiddleware>();
    })

    .Build();

host.Run();
