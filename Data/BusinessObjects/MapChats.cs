using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_chats")]
    [Index(nameof(CounterId), Name = "counter_id")]
    [Index(nameof(MapId), nameof(CounterId), Name = "map_id")]
    public partial class MapChats
    {
        public MapChats()
        {
            MapChatElements = new HashSet<MapChatElements>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("counter_id", TypeName = "int(10) unsigned")]
        public uint? CounterId { get; set; }
        [Required]
        [Column("stem", TypeName = "text")]
        public string Stem { get; set; }
        [Column("is_private", TypeName = "int(4)")]
        public int IsPrivate { get; set; }

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.MapChats))]
        public virtual Maps Map { get; set; }
        [InverseProperty("Chat")]
        public virtual ICollection<MapChatElements> MapChatElements { get; set; }
    }
}
