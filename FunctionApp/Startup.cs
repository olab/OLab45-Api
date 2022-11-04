using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Azure.Identity;
using FluentValidation;
using OLab.FunctionApp.Api;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Utils;
using Microsoft.EntityFrameworkCore;
using OLabWebAPI.Model;

[assembly: FunctionsStartup(typeof(Startup))]

namespace OLab.FunctionApp.Api
{
  [ExcludeFromCodeCoverage]
  public class Startup : FunctionsStartup
  {
    // This method gets called by the runtime. Use this method to add services to the container.
    // public void ConfigureServices(IServiceCollection services)
    // {
    // services.AddSignalR();

    // services.AddCors(options =>
    // {
    //   options.AddPolicy("CorsPolicy",
    //      builder => builder
    //       // .AllowAnyOrigin()
    //       .WithOrigins("http://localhost:4000", "http://localhost:3000", "https://dev.olab.ca", "https://demo.olab.ca")
    //       .AllowAnyMethod()
    //       .AllowAnyHeader()
    //       .AllowCredentials()
    //     );
    // });

    // services.AddControllers().AddNewtonsoftJson();
    // services.AddLogging();

    // Additional code to register the ILogger as a ILogger<T> where T is the Startup class
    // services.AddSingleton(typeof(ILogger), typeof(Logger<Startup>));

    // configure strongly typed settings object
    // services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

    // var serverVersion = ServerVersion.AutoDetect("Server=\"olab45db.mysql.database.azure.com\";UserID=\"olab4admin\";Password=\"lZM398KbCaPQhyOPtwF4!\";Database=\"olab45\";}");
    // services.AddDbContext<OLabDBContext>(
    //     dbContextOptions => dbContextOptions
    //         .UseMySql("Server=\"olab45db.mysql.database.azure.com\";UserID=\"olab4admin\";Password=\"lZM398KbCaPQhyOPtwF4!\";Database=\"olab45\";}", serverVersion)
    //         // The following three options help with debugging, but should
    //         // be changed or removed for production.
    //         // .LogTo(Console.WriteLine, LogLevel.Information)
    //         // .EnableSensitiveDataLogging()
    //         .EnableDetailedErrors()
    //   );

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
    // OLabJWTService.Setup(Configuration, services);

    // define instances of application services
    // services.AddScoped<IUserService, OLabUserService>();
    // services.AddScoped<IOLabSession, OLabSession>();
    // services.AddSingleton<Conference>();
    // }

    public override void Configure(IFunctionsHostBuilder builder)
    {
      // builder
      //     .Services.AddOptions<FileGeneratorOptions>()
      //     .Configure<IConfiguration>((options, c) => { c.GetSection("DPS").Bind(options); });
      // builder
      //     .Services.AddOptions<ProvisioningServiceManagerOptions>()
      //     .Configure<IConfiguration>((options, c) => { c.GetSection("DPS").Bind(options); });
      // builder
      //     .Services.AddOptions<IoTHubProvisionerOptions>()
      //     .Configure<IConfiguration>((options, c) => { c.GetSection("IoTHub").Bind(options); });
      // builder
      //     .Services.AddOptions<StorageOptions>()
      //     .Configure<IConfiguration>((options, c) => { c.GetSection("Storage").Bind(options); });

      // builder.Services.AddOidcApiAuthorization();

      // Azure service connections
      // var configuration = builder.GetContext().Configuration;
      // builder.Services.AddAzureClients(x =>
      // {
      //     x.AddSecretClient(configuration.GetSection("KeyVault"));
      //     x.AddTableServiceClient(configuration.GetSection("Storage"));
      //     x.UseCredential(new DefaultAzureCredential());
      // });

      builder.Services.AddLogging();

      // Azure service connection managers
      // builder.Services.AddSingleton<IDeviceTemplateRepository, DeviceTemplateRepository>();
      // builder.Services.AddSingleton<ISecretManager, SecretManager>();
      // builder.Services.AddTransient<IProvisioningServiceManager, ProvisioningServiceManager>();
      // builder.Services.AddTransient<IIoTHubRegistryManager, IoTHubRegistryManager>();

      // Custom classes
      // builder.Services.AddTransient<IDeviceProvisioningFileGenerator, DeviceProvisioningFileGenerator>();
      // builder.Services.AddTransient<IDpsProvisioner, DpsProvisioner>();
      // builder.Services.AddTransient<IIoTHubProvisioner, IoTHubProvisioner>();

      // Validators
      // builder.Services.AddScoped<
      //     IValidator<DeviceProvisioningFileRequest>,
      //     DeviceProvisioningFileRequestValidator>();
      // builder.Services.AddScoped<
      //     IValidator<DeviceProvisioningRequest>,
      //     DeviceProvisioningRequestValidator>();
      // builder.Services.AddScoped<
      //     IValidator<ProvisionProfileRecordRequest>,
      //     ProvisionProfileRecordRequestValidator>();                
      // builder.Services.AddScoped<
      //     IValidator<CreateProvisionProfileRequest>,
      //     CreateProvisionProfileRequestValidator>();                  
      // builder.Services.AddScoped<
      //     IValidator<ProvisionOrganizationRequest>,
      //     ProvisionOrganizationRequestValidator>();    
      // builder.Services.AddScoped<
      //     IValidator<DeleteProvisionProfileRequest>,
      //     DeleteProvisionProfileRequestValidator>();                               
      // builder.Services.AddScoped<
      //     IValidator<ExecuteQueryRequest>,
      //     ExecuteQueryRequestValidator>();                  
      // builder.Services.AddScoped<
      //     IValidator<GetConfigurationRequest>,
      //     GetConfigurationRequestValidator>();   
      // builder.Services.AddScoped<
      //     IValidator<ExecuteDirectMethodRequest>,
      //     ExecuteDirectMethodRequestValidator>();
      // builder.Services.AddScoped<
      //     IValidator<StreamHistoryModel>,
      //     StreamHistoryValidator>();
      // builder.Services.AddScoped<IValidator<StreamModel>,
      //     StreamValidator>();
      // builder.Services.AddScoped<IValidator<InstalledDeviceModel>,
      //     InstalledDeviceValidator>();
    }
  }
}
