using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_node_section_nodes")]
    [Index(nameof(NodeId), Name = "node_id")]
    [Index(nameof(SectionId), Name = "section_id")]
    public partial class MapNodeSectionNodes
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("section_id", TypeName = "int(10) unsigned")]
        public uint SectionId { get; set; }
        [Column("node_id", TypeName = "int(10) unsigned")]
        public uint NodeId { get; set; }
        [Column("order", TypeName = "int(10)")]
        public int Order { get; set; }
        [Required]
        [Column("node_type", TypeName = "enum('regular','in','out','crucial')")]
        public string NodeType { get; set; }

        [ForeignKey(nameof(NodeId))]
        [InverseProperty(nameof(MapNodes.MapNodeSectionNodes))]
        public virtual MapNodes Node { get; set; }
        [ForeignKey(nameof(SectionId))]
        [InverseProperty(nameof(MapNodeSections.MapNodeSectionNodes))]
        public virtual MapNodeSections Section { get; set; }
    }
}
