using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using OLab.FunctionApp.Api;
using OLabWebAPI.Model;

namespace OLab.Endpoints.Azure
{
  public class UserAzureEndpoint : OLabFunction
  {
    public UserAzureEndpoint(
      ILogger<UserAzureEndpoint> logger,
      IUserService userService,
      OLabDBContext context) : base( logger, userService, context )
    {
    }

    [FunctionName("Login")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")] HttpRequest request,
        ILogger logger,
        CancellationToken cancellationToken)
    {
      Guard.Argument(request).NotNull(nameof(request));
      Guard.Argument(logger).NotNull(nameof(logger));

      logger.LogInformation("C# HTTP trigger function processed a request.");
      var model = await request.ParseBodyFromRequestAsync<LoginRequest>();

      logger.LogDebug($"Login(user = '{model.Username}')");

      var response = userService.Authenticate(model);
      if (response == null)
        return new StatusCodeResult(401);

      return new JsonResult(response);
    }
  }
}
