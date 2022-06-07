using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_node_link_stylies")]
    public partial class MapNodeLinkStylies
    {
        public MapNodeLinkStylies()
        {
            MapNodes = new HashSet<MapNodes>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Required]
        [Column("name")]
        [StringLength(70)]
        public string Name { get; set; }
        [Required]
        [Column("description")]
        [StringLength(500)]
        public string Description { get; set; }

        [InverseProperty("LinkStyle")]
        public virtual ICollection<MapNodes> MapNodes { get; set; }
    }
}
