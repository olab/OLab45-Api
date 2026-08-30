using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Model;
using OLab.Azure.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Azure.Functions;

public class HelloHttp(ILogger<HelloHttp> logger)
{
  [Function( "HelloHttp" )]
  public HttpResponseData Run(
    [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "auth/login" )] HttpRequestData request,
    FunctionContext hostContext,
    CancellationToken cancellationToken)
  {
    logger.LogError( "C# HTTP trigger function processed a request." );
    return OLabFunctionResponses.OLabOkStringResponse( request, "Hello from Azure Functions!" );
  }
}
