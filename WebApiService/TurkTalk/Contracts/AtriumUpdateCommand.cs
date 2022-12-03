using System;
using System.Collections.Generic;
using System.Text.Json;
using OLabWebAPI.Services.TurkTalk.Venue;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  /// <summary>
  /// Defines a Atrium Update command method
  /// </summary>
  public class AtriumUpdateCommand : CommandMethod
  {
    public IList<Learner> Data { get; set; }

    // constructor for targetted group
    public AtriumUpdateCommand(string groupName, IList<Learner> atriumLearners) : base(groupName, "atriumupdate")
    {
      Data = atriumLearners;
    }

    // constructor for all moderators in a topic
    public AtriumUpdateCommand(Topic topic, IList<Learner> atriumLearners) : base(topic.TopicModeratorsChannel, "atriumupdate")
    {
      Data = atriumLearners;
    }

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}