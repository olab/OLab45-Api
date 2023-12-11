using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.BusinessObjects;
using OLab.Data.Interface;
using OLab.FunctionApp.Functions;
using System.Linq;
using System.Net;

namespace OLab.FunctionApp.Functions.API;

public class TestFunction : OLabFunction
{
  public TestFunction(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext) : base(configuration, dbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Logger = OLabLogger.CreateNew<TestFunction>(loggerFactory);
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
    //Logger.LogInformation("LogInformation set");
    //Logger.LogWarning("LogWarning set");
    //Logger.LogError("LogError set");
    //Logger.LogFatal("LogFatal set");

    var response = request.CreateResponse(HttpStatusCode.OK);
    return response;
  }

}
