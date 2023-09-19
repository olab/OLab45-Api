using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
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

    Logger = OLabLogger.CreateNew<TestFunction>(loggerFactory);

    Logger.LogInformation("LogInformation set");
    Logger.LogWarning("LogWarning set");
    Logger.LogError("LogError set");
    Logger.LogFatal("LogFatal set");
  }

  [Function("Bootstrap")]
  public HttpResponseData RunBootstrap(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request)
  {
    var maps = DbContext.Maps.FirstOrDefault(x => x.Id == 0);

    var response = request.CreateResponse(HttpStatusCode.OK);
    return response;
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
