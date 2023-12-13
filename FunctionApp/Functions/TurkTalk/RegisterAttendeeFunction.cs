using DocumentFormat.OpenXml.InkML;
using Microsoft.Azure.Functions.Worker;
using OLab.Api.Common.Contracts;
using OLab.Api.TurkTalk.BusinessObjects;
using OLab.FunctionApp.Utils;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions.SignalR
{
  public partial class TurkTalkFunction : OLabFunction
  {
    [Function("RegisterAttendee")]
    [SignalROutput(HubName = "Hub")]
    public async Task RegisterAttendeeAsync(
      [SignalRTrigger("Hub", "messages", "RegisterAttendee", "payload")] SignalRInvocationContext hostContext,
      TtalkConference conference,
      RegisterAttendeePayload payload)
    {
      payload.ConnectionId = hostContext.ConnectionId;

      var endpoint = new TurkTalkEndpoint(
        Logger,
        _configuration,
        DbContext,
        _ttalkDbContext,
        conference);

      await endpoint.RegisterAttendeeAsync(payload);

      return;
    }
  }
}
