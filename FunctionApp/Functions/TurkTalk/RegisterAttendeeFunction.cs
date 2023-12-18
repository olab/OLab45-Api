using Microsoft.Azure.Functions.Worker;
using OLab.Api.Common.Contracts;
using OLab.TurkTalk.Endpoints;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions.SignalR
{
  public partial class TurkTalkFunction : OLabFunction
  {
    [Function("RegisterAttendee")]
    public async Task<TTalkMessageQueue> RegisterAttendee(
      [SignalRTrigger("Hub", "messages", "RegisterAttendee", "payload")] SignalRInvocationContext invocationContext,
      AttendeePayload payload)
    {
      payload.ConnectionId = invocationContext.ConnectionId;

      var endpoint = new TurkTalkEndpoint(
        Logger,
        _configuration,
        DbContext,
        TtalkDbContext,
        _conference);

      await endpoint.RegisterAttendeeAsync(payload);
      return endpoint.MessageQueue;
    }
  }
}
