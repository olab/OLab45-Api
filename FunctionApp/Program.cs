using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OLab.FunctionApp.Api;
using OLab.FunctionApp.Functions;
using OLab.FunctionApp.Middleware;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;

var host = new HostBuilder()
    .ConfigureAppConfiguration(builder =>
    {
      builder.AddJsonFile(
        "local.settings.json",
        optional: true,
        reloadOnChange: true);
    })

    .ConfigureFunctionsWorkerDefaults(builder =>
    {
      builder.UseMiddleware<OLabAuthMiddleware>();
      builder.UseMiddleware<ExceptionLoggingMiddleware>();
    })

    .ConfigureServices((context, services) =>
    {
      services.AddScoped<IUserService, OLabUserService>();

      var connectionString = Environment.GetEnvironmentVariable("DefaultDatabase");
      var serverVersion = ServerVersion.AutoDetect(connectionString);
      services.AddDbContext<OLabDBContext>(options =>
        options.UseMySql(connectionString, serverVersion).EnableDetailedErrors());

      services.AddOptions<AppSettings>()
          .Configure<IConfiguration>((options, c) => { c.GetSection("AppSettings").Bind(options); });

      services.AddAzureAppConfiguration();

    })
    .Build();

host.Run();
