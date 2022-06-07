using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
  [Table("map_nodes")]
  [Index(nameof(LinkStyleId), Name = "link_style_id")]
  [Index(nameof(MapId), Name = "map_id")]
  public partial class MapNodes
  {
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }
    [Column("map_id", TypeName = "int(10) unsigned")]
    public uint MapId { get; set; }
    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; }
    [Column("text", TypeName = "text")]
    public string Text { get; set; }
    [Column("type_id", TypeName = "int(10) unsigned")]
    public uint? TypeId { get; set; }
    [Column("probability")]
    public bool? Probability { get; set; }
    [Column("conditional")]
    [StringLength(500)]
    public string Conditional { get; set; }
    [Column("conditional_message")]
    [StringLength(1000)]
    public string ConditionalMessage { get; set; }
    [Column("info", TypeName = "text")]
    public string Info { get; set; }
    [Column("is_private", TypeName = "int(4)")]
    public int IsPrivate { get; set; }
    [Column("link_style_id", TypeName = "int(10) unsigned")]
    public uint? LinkStyleId { get; set; }
    [Column("link_type_id", TypeName = "int(10) unsigned")]
    public uint? LinkTypeId { get; set; }
    [Column("priority_id", TypeName = "int(10)")]
    public int? PriorityId { get; set; }
    [Column("kfp")]
    public bool? Kfp { get; set; }
    [Column("undo")]
    public bool? Undo { get; set; }
    [Column("end")]
    public bool? End { get; set; }
    [Column("x")]
    public double? X { get; set; }
    [Column("y")]
    public double? Y { get; set; }
    [Column("rgb")]
    [StringLength(8)]
    public string Rgb { get; set; }
    [Column("show_info", TypeName = "tinyint(4)")]
    public sbyte ShowInfo { get; set; }
    [Column("annotation", TypeName = "text")]
    public string Annotation { get; set; }
    [Column("height", TypeName = "int(10)")]
    public int? Height { get; set; }
    [Column("width", TypeName = "int(10)")]
    public int? Width { get; set; }
    [Column("locked", TypeName = "int(10)")]
    public int? Locked { get; set; }
    [Column("collapsed", TypeName = "int(10)")]
    public int? Collapsed { get; set; }
    [Column("visit_once", TypeName = "int(4)")]
    public int? VisitOnce { get; set; }
    [Column("force_reload", TypeName = "int(10)")]
    public int? ForceReload { get; set; }
    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_At", TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

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

    [ForeignKey(nameof(LinkStyleId))]
    [InverseProperty(nameof(MapNodeLinkStylies.MapNodes))]
    public virtual MapNodeLinkStylies LinkStyle { get; set; }
    [ForeignKey(nameof(MapId))]
    [InverseProperty(nameof(Maps.MapNodes))]
    public virtual Maps Map { get; set; }
    [InverseProperty("Node")]
    public virtual ICollection<MapNodeCounters> MapNodeCounters { get; set; }
    [InverseProperty("Node")]
    public virtual ICollection<MapNodeJumps> MapNodeJumps { get; set; }

    [InverseProperty(nameof(MapNodeLinks.NodeId1Navigation))]
    public virtual ICollection<MapNodeLinks> MapNodeLinksNodeId1Navigation { get; set; }
    [InverseProperty(nameof(MapNodeLinks.NodeId2Navigation))]
    public virtual ICollection<MapNodeLinks> MapNodeLinksNodeId2Navigation { get; set; }
    
    [InverseProperty("MapNode")]
    public virtual ICollection<MapNodeNotes> MapNodeNotes { get; set; }
    [InverseProperty("Node")]
    public virtual ICollection<MapNodeSectionNodes> MapNodeSectionNodes { get; set; }
    [InverseProperty("Node")]
    public virtual ICollection<UserBookmarks> UserBookmarks { get; set; }
    [InverseProperty("Node")]
    public virtual ICollection<UserSessionTraces> UserSessionTraces { get; set; }
    [InverseProperty("MapNode")]
    public virtual ICollection<UserState> UserState { get; set; }
    [InverseProperty("Node")]
    public virtual ICollection<WebinarNodePoll> WebinarNodePoll { get; set; }
    [InverseProperty(nameof(WebinarPoll.OnNodeNavigation))]
    public virtual ICollection<WebinarPoll> WebinarPollOnNodeNavigation { get; set; }
    [InverseProperty(nameof(WebinarPoll.ToNodeNavigation))]
    public virtual ICollection<WebinarPoll> WebinarPollToNodeNavigation { get; set; }

    public enum NodeType : int
    {
      RootNode = 1,
      ContentNode = 2
    }
    
    public MapNodes()
    {
      Constants = new List<SystemConstants>();
      Counters = new List<SystemCounters>();
      Questions = new List<SystemQuestions>();
      Files = new List<SystemFiles>();
      Scripts = new List<SystemScripts>();

      MapNodeCounters = new HashSet<MapNodeCounters>();
      MapNodeJumps = new HashSet<MapNodeJumps>();
      MapNodeLinksNodeId1Navigation = new HashSet<MapNodeLinks>();
      MapNodeLinksNodeId2Navigation = new HashSet<MapNodeLinks>();
      MapNodeNotes = new HashSet<MapNodeNotes>();
      MapNodeSectionNodes = new HashSet<MapNodeSectionNodes>();
      UserBookmarks = new HashSet<UserBookmarks>();
      UserSessionTraces = new HashSet<UserSessionTraces>();
      UserState = new HashSet<UserState>();
      WebinarNodePoll = new HashSet<WebinarNodePoll>();
      WebinarPollOnNodeNavigation = new HashSet<WebinarPoll>();
      WebinarPollToNodeNavigation = new HashSet<WebinarPoll>();
    }
  }
}
