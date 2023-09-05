using System.Net;
using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;

namespace OLab.FunctionApp.Functions;

public class Function1 : OLabFunction
{
  private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

  public Function1(
    ILoggerFactory loggerFactory,
    IConfiguration configuration,
    IUserService userService,
    OLabDBContext dbContext,
    IClaimsPrincipalAccessor claimsPrincipalAccessor) : base( loggerFactory, configuration, userService, dbContext )
  {
    Guard.Argument(claimsPrincipalAccessor).NotNull(nameof(claimsPrincipalAccessor));

    _claimsPrincipalAccessor = claimsPrincipalAccessor;
    var tmp = _configuration.GetValue<string>("Audience");
  }

  [Function("Function1")]
  public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
  {
    Guard.Argument(req).NotNull(nameof(req));

    logger.LogInformation("C# HTTP trigger function processed a request.");

    var response = req.CreateResponse(HttpStatusCode.OK);
    response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

    response.WriteString("Welcome to Azure Functions!");

    return response;
  }
}
