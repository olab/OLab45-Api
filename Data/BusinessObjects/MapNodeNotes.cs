using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_node_notes")]
    [Index(nameof(MapNodeId), Name = "fk_map_node_id_idx")]
    public partial class MapNodeNotes
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_node_id", TypeName = "int(10) unsigned")]
        public uint MapNodeId { get; set; }
        [Column("text", TypeName = "text")]
        public string Text { get; set; }

        [ForeignKey(nameof(MapNodeId))]
        [InverseProperty(nameof(MapNodes.MapNodeNotes))]
        public virtual MapNodes MapNode { get; set; }
    }
}
