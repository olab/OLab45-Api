using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLabWebAPI.Model
{
  public partial class ScopedObjects
  {
        #region Properties

    [NotMapped]
    public List<SystemConstants> Constants { get; set; }
    [NotMapped]
    public List<SystemCounters> Counters { get; set; }
    [NotMapped]
    public List<SystemCounterActions> CounterActions { get; set; }
    [NotMapped]
    public List<SystemFiles> Files { get; set; }
    [NotMapped]
    public List<SystemQuestions> Questions { get; set; }
    [NotMapped]
    public List<SystemScripts> Scripts { get; set; }
    [NotMapped]
    public List<SystemThemes> Themes { get; set; }

    #endregion

    public ScopedObjects()
    {
      Constants = new List<SystemConstants>();
      Counters = new List<SystemCounters>();
      CounterActions = new List<SystemCounterActions>();
      Questions = new List<SystemQuestions>();
      Files = new List<SystemFiles>();
      Scripts = new List<SystemScripts>();
      Themes = new List<SystemThemes>();

      // MapAvatars = new HashSet<MapAvatars>();
      // MapChats = new HashSet<MapChats>();
      // MapCollectionMaps = new HashSet<MapCollectionMaps>();
      // MapContributors = new HashSet<MapContributors>();
      // MapCounterCommonRules = new HashSet<MapCounterCommonRules>();
      // MapDams = new HashSet<MapDams>();
      // MapElements = new HashSet<MapElements>();
      // MapFeedbackRules = new HashSet<MapFeedbackRules>();
      // MapKeys = new HashSet<MapKeys>();
      // MapNodeJumps = new HashSet<MapNodeJumps>();
      // MapNodeLinks = new HashSet<MapNodeLinks>();
      // MapNodeSections = new HashSet<MapNodeSections>();
      // MapUsers = new HashSet<MapUsers>();
      // QCumulative = new HashSet<QCumulative>();
      // ScenarioMaps = new HashSet<ScenarioMaps>();
      // UserSessions = new HashSet<UserSessions>();
      // UserSessionTraces = new HashSet<UserSessionTraces>();
      // UserState = new HashSet<UserState>();
    }

    /// <summary>
    /// Appends a ScopedObjects to the current one
    /// </summary>
    /// <param name="source">Source ScopedObjects</param>
    public void Combine( ScopedObjects source )
    {
      Constants.AddRange( source.Constants );
      Counters.AddRange( source.Counters );
      CounterActions.AddRange( source.CounterActions );
      Questions.AddRange( source.Questions );
      Files.AddRange( source.Files );
      Scripts.AddRange( source.Scripts );
      Themes.AddRange( source.Themes );
    }
  }
}
