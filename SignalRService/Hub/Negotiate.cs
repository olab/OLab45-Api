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
    [FunctionName("negotiate")]
    public SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req)
    {
      var accessToken = req.Headers["Authorization"];

      if (string.IsNullOrEmpty(accessToken))
        return new SignalRConnectionInfo();

      var claims = GetClaims(accessToken);
      var userName = claims.First(c => c.Type == "sub").Value;

      var connectionInfo = Negotiate(
          userName,
          claims
      );

      return connectionInfo;

    }

  }
  
}
