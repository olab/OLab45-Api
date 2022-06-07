using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_collections")]
    public partial class MapCollections
    {
        public MapCollections()
        {
            MapCollectionMaps = new HashSet<MapCollectionMaps>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("name")]
        [StringLength(200)]
        public string Name { get; set; }

        [InverseProperty("Collection")]
        public virtual ICollection<MapCollectionMaps> MapCollectionMaps { get; set; }
    }
}
