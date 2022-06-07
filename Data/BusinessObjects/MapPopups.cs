using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_popups")]
    public partial class MapPopups
    {
        public MapPopups()
        {
            MapPopupsCounters = new HashSet<MapPopupsCounters>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Required]
        [Column("title")]
        [StringLength(300)]
        public string Title { get; set; }
        [Required]
        [Column("text", TypeName = "text")]
        public string Text { get; set; }
        [Column("position_type", TypeName = "int(10)")]
        public int PositionType { get; set; }
        [Column("position_id", TypeName = "int(10) unsigned")]
        public uint PositionId { get; set; }
        [Column("time_before", TypeName = "int(10)")]
        public int TimeBefore { get; set; }
        [Column("time_length", TypeName = "int(10)")]
        public int TimeLength { get; set; }
        [Column("is_enabled", TypeName = "tinyint(4)")]
        public sbyte IsEnabled { get; set; }
        [Column("title_hide", TypeName = "int(10)")]
        public int TitleHide { get; set; }
        [Required]
        [Column("annotation")]
        [StringLength(50)]
        public string Annotation { get; set; }

        [InverseProperty("Popup")]
        public virtual ICollection<MapPopupsCounters> MapPopupsCounters { get; set; }
    }
}
