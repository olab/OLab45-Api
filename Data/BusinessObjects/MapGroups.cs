using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_groups")]
    [Index(nameof(GroupId), Name = "group_id")]
    [Index(nameof(MapId), Name = "map_id")]
    public partial class MapGroups
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("group_id", TypeName = "int(10) unsigned")]
        public uint GroupId { get; set; }
    }
}
