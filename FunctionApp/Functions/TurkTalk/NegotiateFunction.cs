using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace OLab.FunctionApp.Functions.SignalR;

public partial class TurkTalkFunction
{
  [Function("Negotiate")]
  public SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "negotiate")] HttpRequestData req,
      [SignalRConnectionInfoInput(HubName = "Hub")] SignalRConnectionInfo signalRConnectionInfo)
  {
    Logger.LogInformation($"Executing negotiation.");
    return signalRConnectionInfo;
  }
}
