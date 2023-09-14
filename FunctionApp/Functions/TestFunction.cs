using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using System.Net;

namespace OLab.FunctionApp.Functions;

public class TestFunction : OLabFunction
{
  public TestFunction(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    IUserService userService,
    OLabDBContext dbContext) : base(configuration, userService, dbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<ConstantsFunction>(loggerFactory);
    var tmp = _configuration.GetValue<string>("Audience");
  }

  [Function("Health")]
  public HttpResponseData RunHealth(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request,
    FunctionContext hostContext)
  {
    var response = request.CreateResponse(HttpStatusCode.OK);
    return response;
  }

  [Function("Function1")]
  public HttpResponseData Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request,
    FunctionContext hostContext)
  {
    Guard.Argument(request).NotNull(nameof(request));

    Logger.LogInformation("C# HTTP trigger function processed a request.");

    var auth = GetRequestContext(hostContext);

    var response = request.CreateResponse(HttpStatusCode.OK);
    response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

    response.WriteString("Welcome to Azure Functions!");

    return response;
  }

  [Function("Function2")]
  public HttpResponseData Run2(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request,
    FunctionContext hostContext)
  {
    Guard.Argument(request).NotNull(nameof(request));

    Logger.LogInformation("C# HTTP trigger function processed a request.");

    var auth = GetRequestContext(hostContext);

    var response = request.CreateResponse(HttpStatusCode.OK);
    response.Headers.Add("Content-Type", "application/json; charset=utf-8");

    response.WriteString("{\"HelloThere\": 234534 }");

    return response;
  }
}
