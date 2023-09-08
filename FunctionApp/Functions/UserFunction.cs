using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OLab.Api.Common;
using OLab.Api.Model;
using OLab.FunctionApp.Extensions;
using System.Net;

namespace OLab.FunctionApp.Functions;

public class OLabObjectResult2<D>
{
  public static OLabAPIResponse<D> Result(D value)
  {
    var result = new OLabAPIResponse<D>
    {
      Data = value,
      ErrorCode = System.Net.HttpStatusCode.OK
    };

    return result;
  }
}

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
  public async Task<HttpResponseData> LoginAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequestData request,
      CancellationToken cancellationToken)
  {
    Guard.Argument(request).NotNull(nameof(request));

    Logger.LogInformation("C# HTTP trigger function processed a request.");
    var body = await request.ParseBodyFromRequestAsync<LoginRequest>();

    Logger.LogDebug($"Login(user = '{body.Username}')");

    var data = userService.Authenticate(body);

    //var response = request.CreateResponse(HttpStatusCode.OK);
    //response.Headers.Add("Content-Type", "application/json; charset=utf-8");
    //var resp = request.CreateResponse(
    //  OLabObjectResult2<AuthenticateResponse>.Result(data)
    //  );
    //var json = JsonConvert.SerializeObject(tmp);
    //response.WriteString(json);
    //return response;

    var response = request.CreateResponse(OLabObjectResult<AuthenticateResponse>.Result(data));
    return response;

  }
}
