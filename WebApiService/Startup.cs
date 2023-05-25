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
using OLabWebAPI.Data;
using OLabWebAPI.Data.Interface;
using OLabWebAPI.Dto;
using OLabWebAPI.Model;
using OLabWebAPI.Services;
using OLabWebAPI.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using IOLabSession = OLabWebAPI.Data.Interface.IOLabSession;
using IUserService = OLabWebAPI.Services.IUserService;

namespace OLabWebAPI
{

  //public static class MapsterConfiguration
  //{
  //  public static void AddMapster(this IServiceCollection services)
  //  {
  //    var config = TypeAdapterConfig.GlobalSettings;
  //    config.Scan(AppDomain.CurrentDomain.GetAssemblies());
  //  }
  //}

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

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      //services.AddSignalR();

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
        IConfigurationSection config = Configuration.GetSection("Logging");
        builder.ClearProviders();
        builder.AddConsole(configure =>
        {
          configure.FormatterName = ConsoleFormatterNames.Systemd;
        });
        builder.AddConfiguration(config);

      });

      //services.AddMapster();
      //services.AddSingleton(TypeAdapterConfig.GlobalSettings);
      //services.AddScoped<IMapper, ServiceMapper>();

      // Additional code to register the ILogger as a ILogger<T> where T is the Startup class
      services.AddSingleton(typeof(ILogger), typeof(Logger<Startup>));

      // configure strongly typed settings object
      services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

      var serverVersion = ServerVersion.AutoDetect(Configuration.GetConnectionString(Constants.DefaultConnectionStringName));
      services.AddDbContext<OLabDBContext>(
          dbContextOptions => dbContextOptions
              .UseMySql(Configuration.GetConnectionString(Constants.DefaultConnectionStringName), serverVersion)
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

      // MoodleJWTService.Setup(Configuration, services);
      OLabJWTService.Setup(_logger, Configuration, services);

      services.AddTransient<IUserContext, UserContext>();

      // define instances of application services
      services.AddScoped<IUserService, OLabUserService>();
      services.AddScoped<IOLabSession, OLabSession>();
      //services.AddSingleton<Conference>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      // app.UseHttpsRedirection();
      // global cors policy
      app.UseCors("CorsPolicy");
      app.UseRouting();
      app.UseAuthorization();

      // custom jwt auth middleware
      app.UseMiddleware<OLabJWTService>();

      app.UseEndpoints(x =>
      {
        x.MapControllers();
      });

    }
  }
}
