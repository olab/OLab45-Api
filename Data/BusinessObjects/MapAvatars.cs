using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_avatars")]
    [Index(nameof(MapId), Name = "map_id")]
    public partial class MapAvatars
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("skin_1")]
        [StringLength(6)]
        public string Skin1 { get; set; }
        [Column("skin_2")]
        [StringLength(6)]
        public string Skin2 { get; set; }
        [Column("cloth")]
        [StringLength(6)]
        public string Cloth { get; set; }
        [Column("nose")]
        [StringLength(20)]
        public string Nose { get; set; }
        [Column("hair")]
        [StringLength(20)]
        public string Hair { get; set; }
        [Column("environment")]
        [StringLength(20)]
        public string Environment { get; set; }
        [Column("accessory_1")]
        [StringLength(20)]
        public string Accessory1 { get; set; }
        [Column("bkd")]
        [StringLength(6)]
        public string Bkd { get; set; }
        [Column("sex")]
        [StringLength(20)]
        public string Sex { get; set; }
        [Column("mouth")]
        [StringLength(20)]
        public string Mouth { get; set; }
        [Column("outfit")]
        [StringLength(20)]
        public string Outfit { get; set; }
        [Column("bubble")]
        [StringLength(20)]
        public string Bubble { get; set; }
        [Column("bubble_text")]
        [StringLength(100)]
        public string BubbleText { get; set; }
        [Column("accessory_2")]
        [StringLength(20)]
        public string Accessory2 { get; set; }
        [Column("accessory_3")]
        [StringLength(20)]
        public string Accessory3 { get; set; }
        [Column("age")]
        [StringLength(2)]
        public string Age { get; set; }
        [Column("eyes")]
        [StringLength(20)]
        public string Eyes { get; set; }
        [Column("hair_color")]
        [StringLength(6)]
        public string HairColor { get; set; }
        [Column("image")]
        [StringLength(100)]
        public string Image { get; set; }
        [Column("is_private", TypeName = "int(4)")]
        public int IsPrivate { get; set; }

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.MapAvatars))]
        public virtual Maps Map { get; set; }
    }
}
