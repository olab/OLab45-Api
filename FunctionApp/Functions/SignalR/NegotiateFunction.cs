using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.FunctionApp.Extensions;

namespace OLab.FunctionApp.Functions.SignalR
{
  public partial class OLabSignalRFunction
  {
    [Function("Negotiate")]
    public SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "turktalk/negotiate")] HttpRequestData req,
        [SignalRConnectionInfoInput(HubName = "Hub", UserId = "{query.userid}", IdToken = "{query.access_token}")] SignalRConnectionInfo signalRConnectionInfo)
    {
      Logger.LogInformation("Executing negotiation.");
      return signalRConnectionInfo;
    }
  }
}
