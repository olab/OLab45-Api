using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace OLabWebAPI.Model
{
  [Table("maps")]
  public partial class Maps
  {
    public Maps()
    {
      MapNodes = new HashSet<MapNodes>();
      MapNodeLinks = new HashSet<MapNodeLinks>();

      Constants = new List<SystemConstants>();
      Counters = new List<SystemCounters>();
      Questions = new List<SystemQuestions>();
      Files = new List<SystemFiles>();
      Scripts = new List<SystemScripts>();

      SystemCounterActions = new HashSet<SystemCounterActions>();

      MapAvatars = new HashSet<MapAvatars>();
      MapChats = new HashSet<MapChats>();
      MapCollectionMaps = new HashSet<MapCollectionMaps>();
      MapContributors = new HashSet<MapContributors>();
      MapCounterCommonRules = new HashSet<MapCounterCommonRules>();
      MapCounters = new HashSet<MapCounters>();
      MapDams = new HashSet<MapDams>();
      MapElements = new HashSet<MapElements>();
      MapFeedbackRules = new HashSet<MapFeedbackRules>();
      MapKeys = new HashSet<MapKeys>();
      MapNodeJumps = new HashSet<MapNodeJumps>();
      MapNodeLinks = new HashSet<MapNodeLinks>();
      MapNodeSections = new HashSet<MapNodeSections>();
      MapNodes = new HashSet<MapNodes>();
      MapQuestions = new HashSet<MapQuestions>();
      MapUsers = new HashSet<MapUsers>();
      QCumulative = new HashSet<QCumulative>();
      ScenarioMaps = new HashSet<ScenarioMaps>();
      SystemCounterActions = new HashSet<SystemCounterActions>();
      UserSessions = new HashSet<UserSessions>();
      UserSessionTraces = new HashSet<UserSessionTraces>();
      UserState = new HashSet<UserState>();
    }

    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }
    [Required]
    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; }
    [Column("author_id", TypeName = "int(10) unsigned")]
    public uint AuthorId { get; set; }
    [Required]
    [Column("abstract")]
    [StringLength(2000)]
    public string Abstract { get; set; }
    [Column("startScore", TypeName = "int(10)")]
    public int StartScore { get; set; }
    [Column("threshold", TypeName = "int(10)")]
    public int Threshold { get; set; }
    [Required]
    [Column("keywords")]
    [StringLength(500)]
    public string Keywords { get; set; }
    [Column("type_id", TypeName = "int(10) unsigned")]
    public uint TypeId { get; set; }
    [Required]
    [Column("units")]
    [StringLength(10)]
    public string Units { get; set; }
    [Column("security_id", TypeName = "int(10) unsigned")]
    public uint SecurityId { get; set; }
    [Required]
    [Column("guid")]
    [StringLength(50)]
    public string Guid { get; set; }
    [Column("timing")]
    public bool Timing { get; set; }
    [Column("delta_time", TypeName = "int(10)")]
    public int DeltaTime { get; set; }
    [Required]
    [Column("reminder_msg")]
    [StringLength(255)]
    public string ReminderMsg { get; set; }
    [Column("reminder_time", TypeName = "int(10)")]
    public int ReminderTime { get; set; }
    [Column("show_bar")]
    public bool ShowBar { get; set; }
    [Column("show_score")]
    public bool ShowScore { get; set; }
    [Column("skin_id", TypeName = "int(10) unsigned")]
    public uint SkinId { get; set; }
    [Column("enabled")]
    public bool Enabled { get; set; }
    [Column("section_id", TypeName = "int(10) unsigned")]
    public uint SectionId { get; set; }
    [Column("language_id", TypeName = "int(10) unsigned")]
    public uint? LanguageId { get; set; }
    [Required]
    [Column("feedback")]
    [StringLength(2000)]
    public string Feedback { get; set; }
    [Required]
    [Column("dev_notes")]
    [StringLength(1000)]
    public string DevNotes { get; set; }
    [Required]
    [Column("source")]
    [StringLength(50)]
    public string Source { get; set; }
    [Column("source_id", TypeName = "int(10) unsigned")]
    public uint SourceId { get; set; }
    [Column("verification", TypeName = "text")]
    public string Verification { get; set; }
    [Column("assign_forum_id", TypeName = "int(10)")]
    public int? AssignForumId { get; set; }
    [Column("author_rights", TypeName = "int(10)")]
    public int AuthorRights { get; set; }
    [Column("revisable_answers")]
    public bool RevisableAnswers { get; set; }
    [Column("send_xapi_statements")]
    public bool SendXapiStatements { get; set; }
    [Column("renderer_version")]
    public float? RendererVersion { get; set; }
    [Column("is_template", TypeName = "int(10)")]
    public int? IsTemplate { get; set; }
    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_At", TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }


    // public ICollection<MapNodes> MapNodes { get; set; }
    // public ICollection<SystemCounterActions> SystemCounterActions { get; set; }
    // public List<SystemConstants> Constants { get; set; }
    // public List<SystemCounters> Counters { get; set; }
    // public List<SystemFiles> Files { get; set; }
    // public List<SystemQuestions> Questions { get; set; }
    // public List<SystemScripts> Scripts { get; set; }
    // public List<SystemThemes> Themes { get; set; }
    // public ICollection<MapAvatars> MapAvatars { get; set; }
    // public ICollection<MapNodeLinks> MapNodeLinks { get; set; }

    [NotMapped]
    public List<SystemConstants> Constants { get; set; }
    [NotMapped]
    public List<SystemCounters> Counters { get; set; }
    [NotMapped]
    public List<SystemFiles> Files { get; set; }
    [NotMapped]
    public List<SystemQuestions> Questions { get; set; }
    [NotMapped]
    public List<SystemScripts> Scripts { get; set; }
    [NotMapped]
    public List<SystemThemes> Themes { get; set; }

    public virtual Languages Language { get; set; }
    public virtual MapSections Section { get; set; }
    public virtual MapSecurities Security { get; set; }
    public virtual MapTypes Type { get; set; }
    public virtual ICollection<MapAvatars> MapAvatars { get; set; }
    public virtual ICollection<MapChats> MapChats { get; set; }
    public virtual ICollection<MapCollectionMaps> MapCollectionMaps { get; set; }
    public virtual ICollection<MapContributors> MapContributors { get; set; }
    public virtual ICollection<MapCounterCommonRules> MapCounterCommonRules { get; set; }
    public virtual ICollection<MapCounters> MapCounters { get; set; }
    public virtual ICollection<MapDams> MapDams { get; set; }
    public virtual ICollection<MapElements> MapElements { get; set; }
    public virtual ICollection<MapFeedbackRules> MapFeedbackRules { get; set; }
    public virtual ICollection<MapKeys> MapKeys { get; set; }
    public virtual ICollection<MapNodeJumps> MapNodeJumps { get; set; }
    public virtual ICollection<MapNodeLinks> MapNodeLinks { get; set; }
    public virtual ICollection<MapNodeSections> MapNodeSections { get; set; }
    public virtual ICollection<MapNodes> MapNodes { get; set; }
    public virtual ICollection<MapQuestions> MapQuestions { get; set; }
    public virtual ICollection<MapUsers> MapUsers { get; set; }
    public virtual ICollection<QCumulative> QCumulative { get; set; }
    public virtual ICollection<ScenarioMaps> ScenarioMaps { get; set; }
    public virtual ICollection<SystemCounterActions> SystemCounterActions { get; set; }
    public virtual ICollection<UserSessions> UserSessions { get; set; }
    public virtual ICollection<UserSessionTraces> UserSessionTraces { get; set; }
    public virtual ICollection<UserState> UserState { get; set; }
  }
}
