using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Dawn;
using OLabWebAPI.Services.TurkTalk.Contracts;

namespace OLabWebAPI.Services.TurkTalk.Venue
{
  public class Conference
  {
    private readonly ILogger _logger;
    private IDictionary<string, Topic> _topics;
    public ILogger Logger { get { return _logger; } }
    public readonly IHubContext<TurkTalkHub> HubContext;

    public Conference(ILogger logger, IHubContext<TurkTalkHub> hubContext)
    {
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(hubContext).NotNull(nameof(hubContext));

      _logger = logger;
      HubContext = hubContext;
      _topics = new ConcurrentDictionary<string, Topic>();

      logger.LogDebug($"New Conference");
    }

    public async Task AddConnectionToGroupAsync(Participant group)
    {
      Logger.LogDebug($"Added connection '{group.ConnectionId}' to group '{group.Group}'");
      await HubContext.Groups.AddToGroupAsync(group.ConnectionId, group.Group);
    }

    public async Task AddConnectionToGroupAsync( string groupName, string connectionId)
    {
      Logger.LogDebug($"Added connection '{connectionId}' to group '{groupName}'");
      await HubContext.Groups.AddToGroupAsync( connectionId, groupName );
    }

    public async Task RemoveConnectionToGroupAsync( string connectionId, string groupName )
    {
      Logger.LogDebug($"Removing connection '{connectionId}' from group '{groupName}'");
      await HubContext.Groups.RemoveFromGroupAsync( connectionId, groupName );
    }

    /// <summary>
    /// Send message payload to group
    /// </summary>
    /// <param name="groupName">group name id to transmit payload to</param>
    /// <param name="method">message payload</param>
    public void SendMessage(Method method)
    {
      var groupName = method.RecipientGroupName;
      Guard.Argument(groupName).NotEmpty(groupName);

      Logger.LogDebug($"Send message to '{groupName}' ({method.MethodName}): '{method.ToJson()}'");
      HubContext.Clients.Group(groupName).SendAsync(method.MethodName, method);

    }

    /// <summary>
    /// Find/join an unmoderated room for a topic
    /// </summary>
    /// <param name="topicId">Topic id</param>
    /// <param name="create">Flag to create room</param>
    /// <returns>First unmoderated room</returns>
    public Room GetCreateUnmoderatedTopicRoom(string topicId, bool create = true)
    {
      Guard.Argument(topicId).NotEmpty(topicId);

      var topic = GetCreateTopic(topicId);
      return topic.GetCreateUnmoderatedRoom(create);
    }

    /// <summary>
    /// Get/create topic
    /// </summary>
    /// <param name="topicId">Topic Id to get</param>
    /// <param name="create">Optional create, if not exist</param>
    /// <returns>Topic</returns>
    public Topic GetCreateTopic(string topicId, bool create = true)
    {
      Guard.Argument(topicId).NotEmpty(topicId);

      // test if topic doesn't exist yet
      if (!_topics.TryGetValue(topicId, out var topic))
      {
        if (create)
        {
          _topics.Add(topicId, new Topic(this, topicId));
          topic = _topics[topicId];
        }
        else
          topic = null;
      }

      return topic;
    }

  }
}