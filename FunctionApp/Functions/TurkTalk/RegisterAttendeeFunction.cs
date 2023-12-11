using Microsoft.Azure.Functions.Worker;
using OLab.Api.Common.Contracts;
using OLab.TurkTalk.Endpoints;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions.SignalR
{
  public partial class TurkTalkFunction : OLabFunction
  {
    [Function("RegisterAttendee")]
    [SignalROutput(HubName = "Hub")]
    public async Task RegisterAttendeeAsync([SignalRTrigger("Hub", "messages", "SendToGroup", "payload")] SignalRInvocationContext invocationContext,
      RegisterAttendeePayload payload)
    {
      var endpoint = new TurkTalkEndpoint(
        Logger,
        _configuration,
        DbContext,
        TtalkDbContext);

      await endpoint.RegisterAttendeeAsync(payload);

      return;
    }
  }
}
