using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OLabWebAPI
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
    
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging((context, logging) =>
            {
              var config = context.Configuration.GetSection("Logging");
              logging.ClearProviders();
              logging.AddConsole(configure =>
                {
                  configure.Format = Microsoft.Extensions.Logging.Console.ConsoleLoggerFormat.Systemd;
                });
              logging.AddConfiguration(config);
              logging.AddFilter("Microsoft", LogLevel.Error);
              logging.AddFilter("Microsoft.AspNetCore", LogLevel.Error);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
              webBuilder.UseStartup<Startup>();
            });
  }
}
