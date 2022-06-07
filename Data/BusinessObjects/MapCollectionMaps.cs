using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_collectionMaps")]
    [Index(nameof(CollectionId), Name = "collection_id")]
    [Index(nameof(MapId), Name = "map_id")]
    public partial class MapCollectionMaps
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("collection_id", TypeName = "int(10) unsigned")]
        public uint CollectionId { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }

        [ForeignKey(nameof(CollectionId))]
        [InverseProperty(nameof(MapCollections.MapCollectionMaps))]
        public virtual MapCollections Collection { get; set; }
        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.MapCollectionMaps))]
        public virtual Maps Map { get; set; }
    }
}
