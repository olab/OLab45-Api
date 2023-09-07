using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OLab.FunctionApp;
using OLab.FunctionApp.Middleware;
using OLab.FunctionApp.Services;
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
      services.AddScoped<IUserService, FunctionAppUserService>();
      services.AddScoped<IOLabSession, OLabSession>();

      var connectionString = Environment.GetEnvironmentVariable("DefaultDatabase");
      var serverVersion = ServerVersion.AutoDetect(connectionString);
      services.AddDbContext<OLabDBContext>(options =>
        options.UseMySql(connectionString, serverVersion)
          .EnableDetailedErrors());
          //.AddLogging(options => options.SetMinimumLevel(LogLevel.Information));

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

    .ConfigureLogging( builder => 
    {
      builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
    })

    .Build();

host.Run();
