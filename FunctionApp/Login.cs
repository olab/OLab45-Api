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
using OLabWebAPI.Model;

namespace OLab.FunctionApp.Api
{
  public class Login
  {
    private readonly ILogger _logger;
    protected readonly OLabDBContext _context;
    private readonly IUserService _userService;

    public Login(
      IUserService userService,
      ILogger<Login> logger,
      OLabDBContext context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));

      _logger = logger;
      _context = context;
      _userService = userService;
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

      var response = _userService.Authenticate(model);
      if (response == null)
        return new StatusCodeResult(401);

      return new JsonResult(response);
    }
  }
}
