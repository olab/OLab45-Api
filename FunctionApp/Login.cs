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

    public Login(IUserService userService, ILogger<Login> logger, OLabDBContext context)
    {
      _logger = logger;
      _context = context;
      _userService = userService;
    }

    [FunctionName("Login")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")] HttpRequest request,
        ILogger logger,
        CancellationToken cancellationToken)
    {
      _logger.LogInformation("C# HTTP trigger function processed a request.");

      return new OkObjectResult(null);
    }
  }
}
