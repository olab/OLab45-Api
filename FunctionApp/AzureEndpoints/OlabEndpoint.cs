using Dawn;
using Microsoft.Extensions.Logging;
using OLab.FunctionApp.Api;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;

namespace OLab.FunctionApp.Endpoints
{
  public class OLabAzureEndpoint
  {
    protected readonly OLabDBContext context;
    protected OLabLogger logger;
    protected string token;
    protected readonly IUserService userService;

    public OLabAzureEndpoint(ILogger logger, IUserService userService, OLabDBContext context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));

      this.context = context;
      this.logger = new OLabLogger(logger);
      this.userService = userService;
    }
  }
}