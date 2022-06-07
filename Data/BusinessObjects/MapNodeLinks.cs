using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_node_links")]
    [Index(nameof(MapId), Name = "map_id")]
    [Index(nameof(NodeId1), Name = "node_id_1")]
    [Index(nameof(NodeId2), Name = "node_id_2")]
    public partial class MapNodeLinks
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("node_id_1", TypeName = "int(10) unsigned")]
        public uint NodeId1 { get; set; }
        [Column("node_id_2", TypeName = "int(10) unsigned")]
        public uint NodeId2 { get; set; }
        [Column("image_id", TypeName = "int(10) unsigned")]
        public uint? ImageId { get; set; }
        [Column("text")]
        [StringLength(500)]
        public string Text { get; set; }
        [Column("order", TypeName = "int(10)")]
        public int? Order { get; set; }
        [Column("probability", TypeName = "int(10)")]
        public int? Probability { get; set; }
        [Column("hidden")]
        public bool? Hidden { get; set; }
        [Column("link_style_id", TypeName = "int(10) unsigned")]
        public uint? LinkStyleId { get; set; }
        [Column("thickness", TypeName = "int(10)")]
        public int? Thickness { get; set; }
        [Column("line_type", TypeName = "int(10)")]
        public int? LineType { get; set; }
        [Column("color")]
        [StringLength(45)]
        public string Color { get; set; }
        [Column("follow_once", TypeName = "int(4)")]
        public int? FollowOnce { get; set; }
        [Column("created_at", TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }
        [Column("updated_At", TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.MapNodeLinks))]
        public virtual Maps Map { get; set; }

        [ForeignKey(nameof(NodeId1))]
        [InverseProperty(nameof(MapNodes.MapNodeLinksNodeId1Navigation))]
        public virtual MapNodes NodeId1Navigation { get; set; }
        
        [ForeignKey(nameof(NodeId2))]
        [InverseProperty(nameof(MapNodes.MapNodeLinksNodeId2Navigation))]
        public virtual MapNodes NodeId2Navigation { get; set; }
    }
}
