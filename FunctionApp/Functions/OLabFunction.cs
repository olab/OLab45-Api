using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.FunctionApp.Services;
using IOLabAuthentication = OLab.Api.Data.Interface.IOLabAuthentication;

namespace OLab.FunctionApp.Functions;

public class OLabFunction
{
  protected readonly OLabDBContext DbContext;
  protected OLabLogger Logger;
  protected string Token;
  protected readonly IUserService userService;
  protected IUserContext userContext;
  protected IOptions<AppSettings> appSettings;
  protected readonly Configuration _configuration;

  public OLabFunction(
    ILoggerFactory loggerFactory,
    IConfiguration configuration,
    IUserService userService,
    OLabDBContext dbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(userService).NotNull(nameof(userService));
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));

    _configuration = new Configuration(configuration);

    appSettings = Microsoft.Extensions.Options.Options.Create( _configuration.CreateAppSettings() );

    DbContext = dbContext;
    Logger = new OLabLogger(loggerFactory.CreateLogger<OLabFunction>());
    this.userService = userService;
  }

  /// <summary>
  /// Get the authentication context from the host context
  /// </summary>
  /// <param name="hostContext">Function context</param>
  /// <returns>IOLabAuthentication</returns>
  /// <exception cref="Exception"></exception>
  protected IOLabAuthentication GetRequestContext(FunctionContext hostContext)
  {
    // Get the item set by the middleware
    if (hostContext.Items.TryGetValue("auth", out object value) && value is IOLabAuthentication auth)
      Logger.LogInformation("Got auth context");
    else
      throw new Exception("unable to get authentication context");

    return auth;
  }
}