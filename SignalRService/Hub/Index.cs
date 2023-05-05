using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Dawn;
using OLab.SignalRService.BusinessObjects;

namespace OLab.SignalRService.Api
{
  public partial class Hub : ServerlessHub
  {
    [FunctionName("index")]
    public IActionResult Index([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req, ExecutionContext context)
    {
      try
      {
        var path = Path.Combine(context.FunctionAppDirectory, "content", "index.html");
        Console.WriteLine(path);
        return new ContentResult
        {
          Content = File.ReadAllText(path),
          ContentType = "text/html",
        };

      }
      catch (System.Exception ex)
      {
        return new JsonResult( ex );
      }
    }

  }
}
