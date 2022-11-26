using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using OLabWebAPI.Utils;
using Dawn;
using OLabWebAPI.Services.TurkTalk.Contracts;
using OLabWebAPI.Common.Exceptions;

namespace OLabWebAPI.Services.TurkTalk.Venue
{
  /// <summary>
  /// A instance of a topic (to handle when there are
  /// multiple 'rooms' for a topic)
  /// </summary>
  public class Room
  {
    private readonly Topic _topic;
    private int _index;
    private ConcurrentList<Learner> _learnerGroupNames;
    private string _moderatorName;

    public int Index
    {
      get { return _index; }
      private set { _index = value; }
    }

    public string Name { get { return $"room/{_topic.Name}/{Index}"; } }
    public bool IsModerated { get { return !string.IsNullOrEmpty(_moderatorName); } }
    protected ILogger Logger { get { return _topic.Logger; } }

    public Room(Topic topic, int index)
    {
      Guard.Argument(topic).NotNull(nameof(topic));
      _topic = topic;
      _index = index;
      _learnerGroupNames = new ConcurrentList<Learner>(Logger);

      Logger.LogDebug($"New topic '{Name}' instance");
    }

    /// <summary>
    /// Add learner to room
    /// </summary>
    /// <param name="learnerName">Learner user name</param>
    /// <param name="connectionId">Connection id</param>
    internal async Task AddLearner(Learner learnerInfo, string connectionId)
    {
      learnerInfo.AssignToRoom(_index);
      var learnerGroupName = learnerInfo.Group;

      await _topic.Conference.AddConnectionToGroupAsync(learnerGroupName, connectionId);
      _learnerGroupNames.Add(learnerInfo);

      // if ( IsModerated )
      //   _topic.Conference.SendMessage(learnerGroupName, new ModeratorJoinedPayload(_moderatorName));

    }

    /// <summary>
    /// Add moderator to room
    /// </summary>
    /// <param name="moderatorName">Moderator user name</param>
    /// <param name="connectionId">Connection id</param>
    internal async Task AddModeratorAsync(Moderator moderator)
    {
      Guard.Argument(moderator.ConnectionId).NotEmpty(nameof(moderator.ConnectionId));

      if (IsModerated)
        throw new OLabGeneralException($"Room {Name} already moderated.");

      _moderatorName = moderator.NickName;

      // add moderator to its own group so it can receive room assigments
      await _topic.Conference.AddConnectionToGroupAsync(moderator);

      // add new moderator to moderators group (for atrium updates)
      await _topic.Conference.AddConnectionToGroupAsync(
        _topic.ModeratorsGroupName,
        moderator.ConnectionId);

      // notify moderator of room assignment
      _topic.Conference.SendMessage(
        new RoomAssignmentCommand(
          moderator.Group,
          moderator));

      // notify new moderator of atrium contents
      _topic.Conference.SendMessage(
        new AtriumUpdateCommand(
          moderator.Group,
          _topic.Atrium.GetContents()));

    }
  }
}