using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_counters")]
    [Index(nameof(MapId), Name = "map_id")]
    public partial class MapCounters
    {
        public MapCounters()
        {
            MapCounterRules = new HashSet<MapCounterRules>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint? MapId { get; set; }
        [Column("name")]
        [StringLength(200)]
        public string Name { get; set; }
        [Column("description", TypeName = "text")]
        public string Description { get; set; }
        [Column("start_value")]
        public double StartValue { get; set; }
        [Column("icon_id", TypeName = "int(10) unsigned")]
        public uint? IconId { get; set; }
        [Column("prefix")]
        [StringLength(20)]
        public string Prefix { get; set; }
        [Column("suffix")]
        [StringLength(20)]
        public string Suffix { get; set; }
        [Column("visible")]
        public bool? Visible { get; set; }
        [Column("out_of", TypeName = "int(10)")]
        public int? OutOf { get; set; }
        [Column("status", TypeName = "int(1)")]
        public int Status { get; set; }

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.MapCounters))]
        public virtual Maps Map { get; set; }
        [InverseProperty("CounterNavigation")]
        public virtual ICollection<MapCounterRules> MapCounterRules { get; set; }
    }
}
