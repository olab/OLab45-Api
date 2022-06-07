using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_elements")]
    [Index(nameof(MapId), Name = "map_id")]
    public partial class MapElements
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("mime")]
        [StringLength(500)]
        public string Mime { get; set; }
        [Required]
        [Column("name")]
        [StringLength(200)]
        public string Name { get; set; }
        [Required]
        [Column("path")]
        [StringLength(300)]
        public string Path { get; set; }
        [Column("args")]
        [StringLength(100)]
        public string Args { get; set; }
        [Column("width", TypeName = "int(10)")]
        public int? Width { get; set; }
        [Required]
        [Column("width_type")]
        [StringLength(2)]
        public string WidthType { get; set; }
        [Column("height", TypeName = "int(10)")]
        public int? Height { get; set; }
        [Required]
        [Column("height_type")]
        [StringLength(2)]
        public string HeightType { get; set; }
        [Column("h_align")]
        [StringLength(20)]
        public string HAlign { get; set; }
        [Column("v_align")]
        [StringLength(20)]
        public string VAlign { get; set; }
        [Column("is_shared", TypeName = "tinyint(4)")]
        public sbyte IsShared { get; set; }
        [Column("is_private", TypeName = "tinyint(4)")]
        public sbyte IsPrivate { get; set; }

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.MapElements))]
        public virtual Maps Map { get; set; }
    }
}
