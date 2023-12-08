using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OLab.Api.Common.Contracts;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.BusinessObjects.API;
using OLab.Data.Interface;
using OLab.FunctionApp.Functions.API;
using OLab.TurkTalk.Models;
using System.Text.Json;

namespace OLab.FunctionApp.Functions.SignalR;

public partial class OLabSignalRFunction : OLabFunction
{

  [Function("RegisterAttendee")]
  [SignalROutput(HubName = "Hub")]
  public void RegisterAttendee(
    [SignalRTrigger("Hub", "messages", "RegisterAttendee", "payload")] SignalRInvocationContext invocationContext,
    RegisterAttendeePayload payload,
    FunctionContext functionContext)
  {
    Logger.LogInformation($"Executing negotiation. {JsonSerializer.Serialize(payload)}");
  }

}