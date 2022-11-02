using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Azure.Identity;
using FluentValidation;
using OLab.FunctionApp.Api;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// using OidcApiAuthorization;

[assembly: FunctionsStartup(typeof(Startup))]

namespace OLab.FunctionApp.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // builder
            //     .Services.AddOptions<FileGeneratorOptions>()
            //     .Configure<IConfiguration>((options, c) => { c.GetSection("DPS").Bind(options); });

            // builder.Services.AddOidcApiAuthorization();

            // Azure service connections
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddAzureClients(x =>
            {
                x.AddSecretClient(configuration.GetSection("KeyVault"));
                x.AddTableServiceClient(configuration.GetSection("Storage"));
                x.UseCredential(new DefaultAzureCredential());
            });

            builder.Services.AddLogging();

            // Azure service connection managers
            // builder.Services.AddSingleton<IDeviceTemplateRepository, DeviceTemplateRepository>();
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