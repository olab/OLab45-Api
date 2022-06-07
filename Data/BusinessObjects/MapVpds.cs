using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_vpds")]
    [Index(nameof(VpdTypeId), Name = "vpd_type_id")]
    public partial class MapVpds
    {
        public MapVpds()
        {
            MapVpdElements = new HashSet<MapVpdElements>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("vpd_type_id", TypeName = "int(10) unsigned")]
        public uint VpdTypeId { get; set; }

        [ForeignKey(nameof(VpdTypeId))]
        [InverseProperty(nameof(MapVpdTypes.MapVpds))]
        public virtual MapVpdTypes VpdType { get; set; }
        [InverseProperty("Vpd")]
        public virtual ICollection<MapVpdElements> MapVpdElements { get; set; }
    }
}
