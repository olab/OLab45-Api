using Common.Utils;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Services.TurkTalk.Venue;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
    public class TopicAtrium
    {
        public IDictionary<string, Learner> AtriumLearners;
        //public IDictionary<string, AtriumParticipant> AtriumLearners;
        private readonly ILogger _logger;
        private readonly Topic _topic;

        public TopicAtrium(ILogger logger, Topic topic)
        {
            _logger = logger;
            _topic = topic;
            AtriumLearners = new ConcurrentDictionary<string, Learner>();
        }

        /// <summary>
        /// Get list of participant
        /// </summary>
        /// <returns>List of participant group strings</returns>
        public IList<Learner> GetContents()
        {
            return AtriumLearners.Values.ToList();
        }

        /// <summary>
        /// Test if participant already exists in atrium
        /// </summary>
        /// <param name="name">Participant name</param>
        /// <returns>true, if exists</returns>
        public bool Contains(string name)
        {
            return AtriumLearners.ContainsKey(name);
        }

        /// <summary>
        /// Get participant from atrium
        /// </summary>
        /// <param name="name">Participant name</param>
        /// <returns>true, if exists</returns>
        public Learner Get(string name)
        {
            return AtriumLearners[name];
        }

        /// <summary>
        /// Remove participant from atrium
        /// </summary>
        /// <param name="participantName">Participant name</param>
        internal bool Remove(Learner participant)
        {
            bool foundInAtrium = AtriumLearners.ContainsKey(participant.UserId);
            if (foundInAtrium)
            {
                AtriumLearners.Remove(participant.UserId);
                _logger.LogDebug($"Removing participant '{participant.UserId}' ({participant.ConnectionId}) from '{_topic.Name}' atrium");
            }
            else
                _logger.LogDebug($"Participant '{participant.UserId}' not already in '{_topic.Name}' atrium");

            Dump();

            return foundInAtrium;
        }

        /// <summary>
        /// Remove connection id from atrium
        /// </summary>
        /// <param name="connectionId">Connection id to search for</param>
        internal bool Remove(string connectionId)
        {
            foreach (Learner item in AtriumLearners.Values)
            {
                if (item.ConnectionId == connectionId)
                    return Remove(item);
            }

            return false;
        }

        /// <summary>
        /// Add/update participant to atrium
        /// </summary>
        /// <param name="participant">Participant to add</param>
        /// <returns>true if participant replaced (versus just added)</returns>
        public bool Upsert(Learner participant)
        {
            bool replaced = false;

            // remove if already exists
            if (Contains(participant.UserId))
            {
                AtriumLearners.Remove(participant.UserId);
                replaced = true;
            }

            Add(participant);
            Dump();

            return replaced;
        }

        /// <summary>
        /// Add participant to atrium
        /// </summary>
        /// <param name="participant">participant to add</param>
        internal void Add(Learner participant)
        {
            _logger.LogDebug($"Adding participant '{participant.NickName}({participant.UserId})' to '{_topic.Name}' atrium");
            AtriumLearners.Add(participant.UserId, participant);
        }

        private void Dump()
        {
            _logger.LogDebug($"Atrium contents");
            foreach (Learner item in AtriumLearners.Values)
                _logger.LogDebug($"  {item.CommandChannel} ({ConnectionId.Shorten(item.ConnectionId)})");
        }
    }
}
