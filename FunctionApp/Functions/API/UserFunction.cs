using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Access.Interfaces;
using OLab.Api.Common;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.BusinessObjects.API;
using OLab.Data.Interface;
using OLab.FunctionApp.Extensions;
using OLab.FunctionApp.Services;
using System.Net;

namespace OLab.FunctionApp.Functions.API;

public class UserFunction : OLabFunction
{
  protected readonly IUserService _userService;
  private readonly IOLabAuthentication _authentication;

  public UserFunction(
      ILoggerFactory loggerFactory,
      IOLabConfiguration configuration,
      IUserService userService,
      IOLabAuthentication authentication,
      OLabDBContext dbContext) : base(configuration, dbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<UserFunction>(loggerFactory);
    _authentication = authentication;
    _userService = userService;
  }

  [Function("Login")]
  public async Task<HttpResponseData> LoginAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequestData request,
    CancellationToken cancellationToken)
  {
    try
    {
      Guard.Argument(request).NotNull(nameof(request));

      var model = await request.ParseBodyFromRequestAsync<LoginRequest>();

      Logger.LogDebug($"Login(user = '{model.Username}' ip: ???)");

      var user = _userService.Authenticate(model);
      if (user == null)
        return request.CreateResponse(OLabUnauthorizedObjectResult.Result("Username or password is incorrect"));

      var response = _authentication.GenerateJwtToken(user);
      return request.CreateResponse(OLabObjectResult<AuthenticateResponse>.Result(response));

    }
    catch (Exception ex)
    {
      response = request.CreateResponse(ex);
    }

    return response;

  }
}
