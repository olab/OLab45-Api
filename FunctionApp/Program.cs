using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OLab.FunctionApp;
using OLab.FunctionApp.Functions;
using OLab.FunctionApp.Middleware;
using OLabWebAPI.Data.Interface;
using OLabWebAPI.Data;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;
using Microsoft.Extensions.Logging;

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
      services.AddTransient<IUserContext, UserContext>();
      services.AddScoped<IUserService, OLabUserService>();
      services.AddScoped<IOLabSession, OLabSession>();
      services.AddSingleton<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>();

      var connectionString = Environment.GetEnvironmentVariable("DefaultDatabase");
      var serverVersion = ServerVersion.AutoDetect(connectionString);
      services.AddDbContext<OLabDBContext>(options =>
        options.UseMySql(connectionString, serverVersion).EnableDetailedErrors());

      services.AddOptions<AppSettings>()
          .Configure<IConfiguration>((options, c) => { c.GetSection("AppSettings").Bind(options); });

      services.AddAzureAppConfiguration();

    })

    .ConfigureFunctionsWorkerDefaults(builder =>
    {
      builder.UseMiddleware<OLabAuthMiddleware>();
      //builder.UseMiddleware<ExceptionLoggingMiddleware>();
    })

    .Build();

host.Run();
