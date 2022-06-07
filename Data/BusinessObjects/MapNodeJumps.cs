using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_node_jumps")]
    [Index(nameof(MapId), Name = "map_id")]
    [Index(nameof(NodeId), Name = "node_id")]
    public partial class MapNodeJumps
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("node_id", TypeName = "int(10) unsigned")]
        public uint NodeId { get; set; }
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

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.MapNodeJumps))]
        public virtual Maps Map { get; set; }
        [ForeignKey(nameof(NodeId))]
        [InverseProperty(nameof(MapNodes.MapNodeJumps))]
        public virtual MapNodes Node { get; set; }
    }
}
