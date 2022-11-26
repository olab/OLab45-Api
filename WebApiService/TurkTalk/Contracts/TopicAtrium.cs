using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Services.TurkTalk.Venue;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class TopicAtrium
  {
    public IDictionary<string, AtriumLearner> AtriumLearners;
    private ILogger _logger;
    private Topic _topic;

    public TopicAtrium(ILogger logger, Topic topic)
    {
      _logger = logger;
      _topic = topic;
      AtriumLearners = new ConcurrentDictionary<string, AtriumLearner>();
    }

    /// <summary>
    /// Get list of learner
    /// </summary>
    /// <returns>List of learner group strings</returns>
    public IList<AtriumLearner> GetContents()
    {
      return AtriumLearners.Values.ToList();
    }

    /// <summary>
    /// Test if learner already exists in atrium
    /// </summary>
    /// <param name="name">Participant name</param>
    /// <returns>true, if exists</returns>
    public bool Contains(string name)
    {
      return AtriumLearners.ContainsKey(name);
    }

    /// <summary>
    /// Get learner from atrium
    /// </summary>
    /// <param name="name">Participant name</param>
    /// <returns>true, if exists</returns>
    public AtriumLearner Get(string name)
    {
      return AtriumLearners[ name ];
    }

    /// <summary>
    /// Remove learner from atrium
    /// </summary>
    /// <param name="participantName">Participant name</param>
    internal void Remove(string participantName)
    {
      _logger.LogDebug($"Removing '{participantName}' in '{_topic.Name}' atrium");
      AtriumLearners.Remove(participantName);
      Dump();
    }

    /// <summary>
    /// Add/update learner to atrium
    /// </summary>
    /// <param name="participant">Participant to add</param>
    /// <returns>true if learner replaced (versus just added)</returns>
    public bool Upsert(LearnerGroupName participant)
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
    /// Add learner to atrium
    /// </summary>
    /// <param name="learner">learner to add</param>
    internal void Add(LearnerGroupName learner)
    {
      _logger.LogDebug($"Adding learner '{learner.NickName}({learner.UserId})' to '{_topic.Name}' atrium");
      AtriumLearners.Add(learner.UserId, new AtriumLearner( learner ) );
    }

    private void Dump()
    {
      _logger.LogDebug($"Atrium contents");
      foreach (var item in AtriumLearners.Values)
        _logger.LogDebug($"  {item.ToString()}");
    }
  }
}
