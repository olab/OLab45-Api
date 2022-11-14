using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OLab.FunctionApp.Api;
using OLab.FunctionApp.Api.Services;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;

namespace OLab.Endpoints.Azure
{
  public class OLabAzureEndpoint
  {
    protected readonly OLabDBContext context;
    protected OLabLogger logger;
    protected string token;
    protected readonly IUserService userService;
    protected OLabWebApiAuthorization auth;
    protected UserContext userContext;
    
    public OLabAzureEndpoint(ILogger logger, IUserService userService, OLabDBContext context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));

      this.context = context;
      this.logger = new OLabLogger(logger);
      this.userService = userService;
    }

    protected void AuthorizeRequest(HttpRequest request)
    {
      // validate user access token.  throws if not successful
      userService.ValidateToken(request);
      auth = new OLabWebApiAuthorization(logger, context, request);
      userContext = new UserContext(logger, context, request);

    }
  }
}