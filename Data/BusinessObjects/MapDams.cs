using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_dams")]
    [Index(nameof(MapId), Name = "map_id")]
    public partial class MapDams
    {
        public MapDams()
        {
            MapDamElements = new HashSet<MapDamElements>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("name")]
        [StringLength(500)]
        public string Name { get; set; }
        [Column("is_private", TypeName = "int(4)")]
        public int IsPrivate { get; set; }

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.MapDams))]
        public virtual Maps Map { get; set; }
        [InverseProperty("Dam")]
        public virtual ICollection<MapDamElements> MapDamElements { get; set; }
    }
}
