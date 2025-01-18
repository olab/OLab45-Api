using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using System.Net;

namespace OLab.Azure.Functions
{
  public class TestFunction : OLabFunction
  {
    public TestFunction(
      ILoggerFactory loggerFactory,
      IOLabConfiguration configuration,
      OLabDBContext dbContext) : base( configuration, dbContext )
    {
      Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );
      Logger = OLabLogger.CreateNew<TestFunction>( loggerFactory );
    }

    [Function( "Bootstrap" )]
    public HttpResponseData RunBootstrap(
      [HttpTrigger( AuthorizationLevel.Anonymous, "get" )] HttpRequestData request)
    {
      var maps = DbContext.Maps.FirstOrDefault( x => x.Id > 0 );

      var response = request.CreateResponse( HttpStatusCode.OK );
      return response;
    }

    [Function( "HealthCheck" )]
    public IActionResult Run([HttpTrigger( AuthorizationLevel.Function, "get" )] HttpRequest req)
    {
      Logger.LogInformation( "C# HTTP trigger function processed a request." );
      return new OkObjectResult( "Welcome to Azure Functions!" );
    }
  }
}
