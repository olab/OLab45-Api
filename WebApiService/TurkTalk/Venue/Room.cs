using Common.Utils;
using Dawn;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Services.TurkTalk.Contracts;
using OLabWebAPI.TurkTalk.Contracts;
using OLabWebAPI.Utils;
using System.Threading.Tasks;

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
    private readonly ConcurrentList<Learner> _learners;
    private Moderator _moderator = null;

    public int Index
    {
      get { return _index; }
      private set { _index = value; }
    }

    public Moderator Moderator { get { return _moderator; } }
    public string Name { get { return $"{_topic.Name}/{Index}"; } }
    public bool IsModerated { get { return _moderator != null; } }
    protected ILogger Logger { get { return _topic.Logger; } }

    public Room(Topic topic, int index)
    {
      Guard.Argument(topic).NotNull(nameof(topic));
      _topic = topic;
      _index = index;
      _learners = new ConcurrentList<Learner>(Logger);

      Logger.LogDebug($"New room '{Name}'");
    }

    /// <summary>
    /// Add learner to room
    /// </summary>
    /// <param name="learnerName">Learner user name</param>
    /// <param name="connectionId">Connection id</param>
    internal async Task AddLearnerAsync(Learner learner, string connectionId)
    {
      learner.AssignToRoom(_index);

      await _topic.Conference.AddConnectionToGroupAsync(learner.CommandChannel, connectionId);

      _learners.Add(learner);

      Logger.LogDebug($"Added learner {learner} to room '{Name}'");


    }

    /// <summary>
    /// Add moderator to room
    /// </summary>
    /// <param name="moderatorName">Moderator user name</param>
    /// <param name="connectionId">Connection id</param>
    internal async Task AddModeratorAsync(Moderator moderator)
    {
      if (!IsModerated)
        _moderator = moderator;

      // add new moderator to moderators group (for atrium updates)
      await _topic.Conference.AddConnectionToGroupAsync(
        _topic.TopicModeratorsChannel,
        moderator.ConnectionId);

      // add new moderator to its own group so it can receive messages
      await _topic.Conference.AddConnectionToGroupAsync(moderator);

      // notify moderator of room assignment
      _topic.Conference.SendMessage(
        new RoomAssignmentCommand(
          moderator.CommandChannel,
          moderator));

      // notify moderator of atrium contents
      _topic.Conference.SendMessage(
        new AtriumUpdateCommand(
          moderator.CommandChannel,
          _topic.Atrium.GetContents()));

      // notify moderator of already assigned learners
      _topic.Conference.SendMessage(
        new LearnerListCommand(
          moderator.CommandChannel,
          _learners.Items));

      foreach (var learner in _learners.Items)
      {
        // notify all known learners in room of
        // moderator (re)connection
        _topic.Conference.SendMessage(
            new RoomAssignmentCommand(
              learner.CommandChannel,
              learner));
      }

    }

    /// <summary>
    /// Signals a disconnection of a signalr session
    /// </summary>
    /// <param name="connectionId"></param>
    internal void RemoveParticipant(Participant participant)
    {
      Logger.LogDebug($"Disconnecting {ConnectionId.Shorten(participant.ConnectionId)} from room '{Name}'");

      if (!IsModerated)
      {
        Logger.LogInformation($"Room {Name} is not already moderated");
        return;
      }

      // test if participant to remove is the moderator
      if (participant.ConnectionId == _moderator.ConnectionId)
      {
        Logger.LogDebug($"Participant '{participant.UserId}' ({ConnectionId.Shorten(participant.ConnectionId)}) is a moderator for room '{Name}'. removing.");

        // notify all known learners in room of moderator disconnection
        foreach (var learner in _learners.Items)
          _topic.Conference.SendMessage(
            new ModeratorDisconnectedCommand(learner.CommandChannel));

        // the moderator has left the buiding
        _moderator = null;

      }
      else
      {
        // notify room moderator of unassignment 
        // of (potential) learner from connectionId
        _topic.Conference.SendMessage(
          new RoomUnassignmentCommand(
            _moderator.CommandChannel,
            participant));
      }

    }

  }
}