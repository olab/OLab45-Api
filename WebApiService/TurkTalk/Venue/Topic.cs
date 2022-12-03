using Dawn;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Services.TurkTalk.Contracts;
using OLabWebAPI.Utils;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OLabWebAPI.Services.TurkTalk.Venue
{
    /// <summary>
    /// Chat topic
    /// </summary>
    public class Topic
    {
        private readonly Conference _conference;
        private readonly ConcurrentList<Room> _instances;
        private string _name;
        public string TopicModeratorsChannel;

        // Needed because there's no such thing as a thread-safe List<>.
        private static readonly Mutex roomMutex = new Mutex();

        public Conference Conference
        {
            get { return _conference; }
        }

        public string Name
        {
            get { return _name; }
            private set { _name = value; }
        }

        // public IDictionary<string, LearnerGroupName> AtriumLearners;
        public TopicAtrium Atrium;
        public ConcurrentList<Room> Rooms { get { return _instances; } }
        public ILogger Logger { get { return _conference.Logger; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="conference"></param>
        /// <param name="topicId"></param>
        public Topic(Conference conference, string topicId)
        {
            Guard.Argument(conference).NotNull(nameof(conference));

            _conference = conference;
            _instances = new ConcurrentList<Room>(Logger);

            Name = topicId;
            // AtriumLearners = new ConcurrentDictionary<string, LearnerGroupName>();
            Atrium = new TopicAtrium(Logger, this);

            TopicModeratorsChannel = $"{Name}/moderators";

            Logger.LogDebug($"New topic '{Name}'");
        }

        /// <summary>
        /// Crfeate room in topic
        /// </summary>
        /// <returns></returns>
        public Room CreateRoom()
        {
            Room room = new Room(this, _instances.Count);
            _instances.Add(room);

            int index = _instances.Count - 1;
            return _instances[index];
        }

        /// <summary>
        /// Get first existing or new/unmoderated room
        /// </summary>
        /// <param name="moderator">Moderator requesting room</param>
        /// <returns>Room instance of topic</returns>
        public Room GetCreateUnmoderatedRoom(Moderator moderator)
        {
            Room room = null;
            roomMutex.WaitOne();

            // test if moderator was already assigned to room
            if (moderator.IsAssignedToRoom())
            {
                room = Rooms.Items.FirstOrDefault(x => x.Index == moderator.RoomNumber);
                if (room != null)
                    Logger.LogDebug($"Returning previously open room '{moderator.RoomName}'");
                else
                    Logger.LogDebug($"Previously assigned room '{moderator.RoomName}' no longer exists");
            }
            else
            {
                room = Rooms.Items.Where(x => !x.IsModerated).FirstOrDefault();
                if (room != null)
                    Logger.LogDebug($"Returning first unmoderated room '{room.Name}/{room.Index}'");
                else
                    Logger.LogDebug($"No existing, unmoderated rooms for '{moderator.RoomName}'.");
            }

            if (room == null)
                room = CreateRoom();

            roomMutex.ReleaseMutex();

            return room;
        }

        /// <summary>
        /// Get number of rooms in session
        /// </summary>
        /// <returns>Room count</returns>
        public int RoomCount()
        {
            roomMutex.WaitOne();
            int count = _instances.Count;
            roomMutex.ReleaseMutex();

            return count;
        }

        /// <summary>
        /// Get session room by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Room</returns>
        public Room GetRoom(int index)
        {
            roomMutex.WaitOne();
            if (index >= Rooms.Count)
                throw new ArgumentOutOfRangeException("Invalid topic room instance argument");
            Room room = _instances[index];
            roomMutex.ReleaseMutex();

            return room;
        }

        /// <summary>
        /// Remove user from atrium 
        /// </summary>
        /// <param name="participant">Learner to remove</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal void RemoveFromAtrium(Learner participant)
        {
            // try and remove learner.  if removed, notify all topic
            // moderators of atrium change
            if (Atrium.Remove(participant))
                Conference.SendMessage(
                  new AtriumUpdateCommand(this, Atrium.GetContents()));
        }

        /// <summary>
        /// Removes a connection from the topic
        /// </summary>
        /// <param name="connectionId"></param>
        internal void RemoveConnection(string connectionId)
        {
            // first remove from atrium
            RemoveFromAtrium(connectionId);

            // go thru each room and signal a disconnected
            // connection id
            foreach (Room room in Rooms.Items)
                room.RemoveConnection(connectionId);

        }

        /// <summary>
        /// Remove connection id from atrium 
        /// </summary>
        /// <param name="connectionId">Connection remove</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal void RemoveFromAtrium(string connectionId)
        {
            // try and remove connection.  if removed, notify all topic
            // moderators of atrium change
            if (Atrium.Remove(connectionId))
                Conference.SendMessage(
                  new AtriumUpdateCommand(this, Atrium.GetContents()));
        }

        /// <summary>
        /// Add participant to topic atrium
        /// </summary>
        /// <param name="participant">Leaner info</param>
        /// <param name="connectionId">Connection id</param>
        internal async Task AddToAtriumAsync(Learner participant)
        {
            // add/replace participant in atrium
            bool learnerReplaced = Atrium.Upsert(participant);

            // if replaced a atrium contents, remove it from group
            if (learnerReplaced)
            {
                Logger.LogDebug($"Replacing existing '{Name}' atrium participant '{participant.CommandChannel}'");
                await Conference.RemoveConnectionToGroupAsync(participant.ConnectionId, participant.CommandChannel);
            }

            // add participant to its own group so it can receive room assigments
            await Conference.AddConnectionToGroupAsync(participant);

            // notify participant of atrium assignment
            Conference.SendMessage(
              new AtriumAssignmentCommand(participant, Atrium.Get(participant.UserId)));

            // notify all topic moderators of atrium change
            Conference.SendMessage(
              new AtriumUpdateCommand(this, Atrium.GetContents()));

        }

    }
}