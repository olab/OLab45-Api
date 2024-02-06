//using Mapster;
//using MapsterMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Api.Common;
using OLab.Data.Models;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data;
using OLab.Data.Interface;
using OLabWebAPI.Services;

namespace OLabWebAPI
{

  public class Startup
  {
    public IConfiguration Configuration { get; }
    private readonly ILogger<Startup> _logger;

    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
      using var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder.SetMinimumLevel(LogLevel.Information);
        builder.AddConsole();
        builder.AddEventSourceLogger();
      });
      _logger = loggerFactory.CreateLogger<Startup>();
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services">Services collection</param>
    public void ConfigureServices(IServiceCollection services)
    {
      //services.AddSignalR();
      JsonConvert.DefaultSettings = () => new JsonSerializerSettings
      {
#if DEBUG
        Formatting = Formatting.Indented,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
#endif
      };

      services.AddCors(options =>
      {
        options.AddPolicy("CorsPolicy",
           builder => builder
            // .AllowAnyOrigin()
            .WithOrigins("http://localhost:4000", "http://localhost:3000", "https://dev.olab.ca", "https://demo.olab.ca")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
          );
      });

      services.AddControllers().AddNewtonsoftJson();
      services.AddLogging(builder =>
      {
        var config = Configuration.GetSection("Logging");
        builder.ClearProviders();
        builder.AddConsole(configure =>
        {
          configure.FormatterName = ConsoleFormatterNames.Systemd;
        });
        builder.AddConfiguration(config);

      });

      // Additional code to register the ILogger as a ILogger<T> where T is the Startup class
      services.AddSingleton(typeof(ILogger), typeof(Logger<Startup>));

      // configure strongly typed settings object
      services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

      var connectionString = Configuration.GetConnectionString(ConstantStrings.DefaultConnectionStringName);
      var serverVersion = ServerVersion.AutoDetect(connectionString);
      services.AddDbContext<OLabDBContext>(options =>
        options.UseMySql(connectionString, serverVersion)
          // The following three options help with debugging, but should
          // be changed or removed for production.
          // .LogTo(Console.WriteLine, LogLevel.Information)
          // .EnableSensitiveDataLogging()
          .EnableDetailedErrors()
        );

      // Everything from this point on is optional but helps with debugging.
      // .UseLoggerFactory(
      //     LoggerFactory.Create(
      //         logging => logging
      //             .AddConsole()
      //             .AddFilter(level => level >= LogLevel.Information)))
      // .EnableSensitiveDataLogging()
      // .EnableDetailedErrors()
      // );

      // set up JWT authenticatio services
      // Build the intermediate service provider
#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
      var sp = services.BuildServiceProvider();
#pragma warning restore ASP0000 
      var dbContext = sp.GetService<OLabDBContext>();
      OLabAuthMiddleware.SetupServices(Configuration, services, dbContext);

      //services.AddScoped<IOLabSession, OLabSession>();
      services.AddScoped<IUserService, UserService>();
      services.AddScoped<IOLabAuthentication, OLabAuthentication>();
      services.AddScoped<IOLabAuthorization, OLabAuthorization>();

      // define instances of application services
      services.AddSingleton<IOLabLogger, OLabLogger>();
      services.AddSingleton<IOLabConfiguration, OLabConfiguration>();

      services.AddSingleton(typeof(IOLabModuleProvider<>), typeof(OLabModuleProvider<>));
      services.AddSingleton<IOLabModuleProvider<IWikiTagModule>, WikiTagProvider>();
      services.AddSingleton<IOLabModuleProvider<IFileStorageModule>, FileStorageProvider>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
        app.UseDeveloperExceptionPage();

      // app.UseHttpsRedirection();
      // global cors policy
      app.UseCors("CorsPolicy");
      app.UseRouting();
      app.UseAuthorization();

      // custom jwt auth middleware
      app.UseMiddleware<OLabAuthMiddleware>();

      app.UseEndpoints(x =>
      {
        x.MapControllers();
      });

    }
  }
}
