using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLab.FunctionApp.Api;
using OLabWebAPI.Model;

namespace OLab.FunctionApp.Functions
{
  public class UserFunction : OLabFunction
  {
    public UserFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base( loggerFactory, configuration, userService, dbContext )
    {
      Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
      Guard.Argument(configuration).NotNull(nameof(configuration));
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(userService).NotNull(nameof(userService));
    }

    [Function("Login")]
    public async Task<IActionResult> LoginAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequest request,
        ILogger logger,
        CancellationToken cancellationToken)
    {
      Guard.Argument(request).NotNull(nameof(request));
      Guard.Argument(logger).NotNull(nameof(logger));

      logger.LogInformation("C# HTTP trigger function processed a request.");
      var body = await request.ParseBodyFromRequestAsync<LoginRequest>();

      logger.LogDebug($"Login(user = '{body.Username}')");

      var response = userService.Authenticate(body);
      if (response == null)
        return new StatusCodeResult(401);

      return new JsonResult(response);
    }
  }
}
