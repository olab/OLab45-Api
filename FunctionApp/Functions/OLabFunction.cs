using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OLab.FunctionApp.Api;
using OLabWebAPI.Data;
using OLabWebAPI.Data.Interface;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;
using IOLabAuthentication = OLabWebAPI.Data.Interface.IOLabAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace OLab.FunctionApp.Functions
{
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
      Guard.Argument(configuration).NotNull(nameof(configuration));
      Guard.Argument(dbContext).NotNull(nameof(dbContext));

      _configuration = new Configuration(configuration);

      context = dbContext;
      logger = new OLabLogger(loggerFactory.CreateLogger<OLabFunction>());
      this.userService = userService;
    }

    //protected IOLabAuthentication AuthorizeRequest(HttpRequest request)
    //{
    //  logger.LogInformation($"Authorizing request");

    //  // validate user access token.  throws if not successful
    //  userService.ValidateToken(request);
    //  userContext = new UserContext(logger, context, request);

    //  logger.LogInformation($"Request authorized");

    //  return auth;
    //}
  }
}