using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using OLabWebAPI.Utils;
using OLabWebAPI.Services;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Model;
using OLabWebAPI.Data.Session;
using TurkTalk.Contracts;

namespace OLabWebAPI
{
  public class Startup
  {
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddSignalR();

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
      services.AddLogging();

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
      OLabJWTService.Setup(Configuration, services);

      // define instances of application services
      services.AddScoped<IUserService, OLabUserService>();
      services.AddScoped<IOLabSession, OLabSession>();
      services.AddSingleton<Conference>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      // app.UseHttpsRedirection();
      app.UseRouting();
      app.UseAuthorization();

      // global cors policy
      app.UseCors("CorsPolicy");

      // custom jwt auth middleware
      app.UseMiddleware<OLabJWTService>();

      app.UseEndpoints(x =>
      {
        x.MapControllers();
        x.MapHub<TurkTalkHub>("/turktalk");
      });

    }
  }
}
