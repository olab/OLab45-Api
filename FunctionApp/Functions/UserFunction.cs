using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Functions;

public class UserFunction : OLabFunction
{
  public UserFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base(configuration, userService, dbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<UserFunction>(loggerFactory);
  }

  [Function("Login")]
  public async Task<HttpResponseData> LoginAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequestData request,
    CancellationToken cancellationToken)
  {
    try
    {
      Guard.Argument(request).NotNull(nameof(request));


      var body = await request.ParseBodyFromRequestAsync<LoginRequest>();

      Logger.LogDebug($"Login(user = '{body.Username}')");

      var data = userService.Authenticate(body);

      if (data == null)
        response = request.CreateResponse(
          OLabUnauthorizedObjectResult.Result("Username or password is incorrect"));
      else
        response = request.CreateResponse(OLabObjectResult<AuthenticateResponse>.Result(data));

    }
    catch (Exception ex)
    {
      response = request.CreateResponse(ex);
    }

    return response;

  }
}
