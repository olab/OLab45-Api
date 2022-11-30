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

            Logger.LogDebug($"New room '{Name}'");
        }

        /// <summary>
        /// Add learner to room
        /// </summary>
        /// <param name="learnerName">Learner user name</param>
        /// <param name="connectionId">Connection id</param>
        internal async Task AddLearner(Learner learner, string connectionId)
        {
            learner.AssignToRoom(_index);

            await _topic.Conference.AddConnectionToGroupAsync(learner.CommandChannel, connectionId);
            _learnerGroupNames.Add(learner);

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
            if (!IsModerated)
                _moderatorName = moderator.NickName;

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

            // notify new moderator of atrium contents
            _topic.Conference.SendMessage(
              new AtriumUpdateCommand(
                moderator.CommandChannel,
                _topic.Atrium.GetContents()));

        }
    }
}