using DocumentFormat.OpenXml.InkML;
using Microsoft.Azure.Functions.Worker;
using OLab.Api.Common.Contracts;
using OLab.Api.TurkTalk.BusinessObjects;
using OLab.FunctionApp.Utils;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints;
using OLab.TurkTalk.Endpoints.Interface;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions.SignalR
{
  public partial class TurkTalkFunction : OLabFunction
  {
    [Function("RegisterAttendee")]
    [SignalROutput(HubName = "Hub")]
    public async Task<SignalRMessageAction> RegisterAttendee(
      [SignalRTrigger("Hub", "messages", "RegisterAttendee", "payload")] SignalRInvocationContext invocationContext,
      RegisterAttendeePayload payload)
    {
      payload.ConnectionId = invocationContext.ConnectionId;

      var endpoint = new TurkTalkEndpoint(
        Logger,
        _configuration,
        DbContext,
        _ttalkDbContext,
        _conference);

      await endpoint.RegisterAttendeeAsync(payload);

      return new AtriumAcceptedMethod(
          _configuration,
          invocationContext.ConnectionId,
          payload.RoomName).Message();
    }
  }
}
