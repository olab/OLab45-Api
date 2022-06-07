using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_keys")]
    [Index(nameof(Key), Name = "key")]
    [Index(nameof(MapId), Name = "map_id")]
    public partial class MapKeys
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Required]
        [Column("key")]
        [StringLength(50)]
        public string Key { get; set; }

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.MapKeys))]
        public virtual Maps Map { get; set; }
    }
}
