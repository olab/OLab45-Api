using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLab.Api.Model;
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Functions;

public class UserFunction : OLabFunction
{
  public UserFunction(
    ILoggerFactory loggerFactory,
    IConfiguration configuration,
    IUserService userService,
    OLabDBContext dbContext) : base(loggerFactory, configuration, userService, dbContext)
  {
  }

  [Function("Login")]
  public async Task<IActionResult> LoginAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequestData request,
      CancellationToken cancellationToken)
  {
    Guard.Argument(request).NotNull(nameof(request));

    Logger.LogInformation("C# HTTP trigger function processed a request.");
    var body = await request.ParseBodyFromRequestAsync<LoginRequest>();

    Logger.LogDebug($"Login(user = '{body.Username}')");

    var response = userService.Authenticate(body);
    if (response == null)
      return new StatusCodeResult(401);

    return new JsonResult(response);
  }
}
