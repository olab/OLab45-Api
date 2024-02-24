using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Common.Utils;
using Microsoft.Azure.Functions.Worker;
using OLab.Access;
using OLab.TurkTalk.Data.Contracts;
using OLab.TurkTalk.Endpoints;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions.SignalR;

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class SignalRConnectionStatus
{
  [JsonPropertyName("timestamp")]
  public DateTime Timestamp { get; set; }

  [JsonPropertyName("hubName")]
  public string HubName { get; set; }

  [JsonPropertyName("connectionId")]
  public string ConnectionId { get; set; }

  [JsonPropertyName("userId")]
  public object UserId { get; set; }
}

public partial class TurkTalkFunction : OLabFunction
{
  /// <summary>
  /// Listens for Event Grid triggers and outputs SignalR messages
  /// </summary>
  /// <param name="eventGridEvent">Event Grid event</param>
  /// <returns>DispatchedMessages</returns>
  [Function(nameof(OnDisconnectedEvent))]
  [SignalROutput(HubName = "Hub")]
  public async Task<SignalRMessageAction> OnDisconnectedEvent(
    /* [EventGridTrigger] */ EventGridEvent eventGridEvent,
    CancellationToken cancellation)
  {
    Logger.LogInformation($"Event type: {JsonSerializer.Serialize(eventGridEvent)}");

    var signalRDataString = Encoding.ASCII.GetString(eventGridEvent.Data);
    var signalRData = 
      JsonSerializer.Deserialize<SignalRConnectionStatus>(signalRDataString);

    var payload = new OnDisconnectedRequest();
    payload.ConnectionId = signalRData.ConnectionId;

    var endpoint = new TurkTalkEndpoint(
      Logger,
      _configuration,
      _conference);

    await endpoint.OnDisconnectedAsync(
      _configuration,
      payload,
      cancellation);

    Logger.LogInformation(JsonSerializer.Serialize(endpoint.MessageQueue.Messages));

    var action = 
      endpoint.MessageQueue.Messages.FirstOrDefault() as SignalRMessageAction;

    return action;
  }

}
