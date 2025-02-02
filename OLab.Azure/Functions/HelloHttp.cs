using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace OLab.Azure.Functions;

public class HelloHttp(ILogger<HelloHttp> logger)
  {
      [Function("HelloHttp")]
      public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
      {
          logger.LogError("C# HTTP trigger function processed a request.");
          return new OkObjectResult("Welcome to Azure Functions!");
      }
  }
