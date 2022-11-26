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
    private ConcurrentList<LearnerGroupName> _learnerGroupNames;
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
      _learnerGroupNames = new ConcurrentList<LearnerGroupName>(Logger);

      Logger.LogDebug($"New topic '{Name}' instance");
    }

    /// <summary>
    /// Add learner to room
    /// </summary>
    /// <param name="learnerName">Learner user name</param>
    /// <param name="connectionId">Connection id</param>
    internal async Task AddLearner(LearnerGroupName learnerInfo, string connectionId)
    {
      learnerInfo.AssignToRoom( _index );
      var learnerGroupName = learnerInfo.ToString();

      await _topic.Conference.AddConnectionToGroupAsync(connectionId, learnerGroupName);
      _learnerGroupNames.Add(learnerInfo);
      
      // if ( IsModerated )
      //   _topic.Conference.SendMessage(learnerGroupName, new ModeratorJoinedPayload(_moderatorName));

    }

    /// <summary>
    /// Add moderator to room
    /// </summary>
    /// <param name="moderatorName">Moderator user name</param>
    /// <param name="connectionId">Connection id</param>
    internal async Task AddModeratorAsync(string moderatorName, string connectionId)
    {
      Guard.Argument(moderatorName).NotEmpty(nameof(moderatorName));
      Guard.Argument(connectionId).NotEmpty(nameof(connectionId));

      if (IsModerated)
        throw new OLabGeneralException($"Room {Name} already moderated.");

      _moderatorName = moderatorName;

      // add new moderator to moderators group (for atrium updates)
      await _topic.Conference.AddConnectionToGroupAsync(connectionId, _topic.ModeratorsGroupName);

      Guard.Argument(moderatorName).NotEmpty(moderatorName);

      // manually lock the collection before servicing the learner list
      _learnerGroupNames.Lock();

      // foreach (var learnerGroupName in _learnerGroupNames.Items)
      //   _topic.Conference.SendMessage(learnerGroupName.ToString(), new ModeratorJoinedPayload(moderatorName));

      // notify all moderators of atrium change
      _topic.Conference.SendMessage(
        new AtriumUpdateCommand( _topic.ModeratorsGroupName, _topic.Atrium.GetContents() ));

      _learnerGroupNames.Unlock();

    }
  }
}