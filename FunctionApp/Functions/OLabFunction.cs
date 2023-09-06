using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLab.Data.Interface;
using OLab.Model;
using OLab.Utils;
using IOLabAuthentication = OLab.Data.Interface.IOLabAuthentication;

namespace OLab.FunctionApp.Functions;

public class OLabFunction
{
  protected readonly OLabDBContext context;
  protected OLabLogger logger;
  protected string token;
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

    context = dbContext;
    logger = new OLabLogger(loggerFactory.CreateLogger<OLabFunction>());
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
      logger.LogInformation("Got auth context");
    else
      throw new Exception("unable to get authentication context");

    return auth;
  }
}