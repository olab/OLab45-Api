using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OLab.FunctionApp.Api;
using OLab.FunctionApp.Api.Services;
using OLabWebAPI.Data;
using OLabWebAPI.Data.Interface;
using OLabWebAPI.Dto;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;
using System.IO;
using System.Threading.Tasks;
using IOLabAuthentication = OLabWebAPI.Data.Interface.IOLabAuthentication;

namespace OLab.Endpoints.Azure
{
  public class OLabFunction
  {
    protected readonly OLabDBContext context;
    protected OLabLogger logger;
    protected string token;
    protected readonly IUserService userService;
    protected IUserContext userContext;

    public OLabFunction(ILogger logger, IUserService userService, OLabDBContext context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));

      this.context = context;
      this.logger = new OLabLogger(logger);
      this.userService = userService;
    }

    protected IOLabAuthentication AuthorizeRequest(HttpRequest request)
    {
      logger.LogInformation($"Authorizing request");

      // validate user access token.  throws if not successful
      userService.ValidateToken(request);

      var auth = new OLabAuthorization(logger, context, request);
      userContext = new UserContext(logger, context, request);

      logger.LogInformation($"Request authorized");

      return auth;
    }
  }
}