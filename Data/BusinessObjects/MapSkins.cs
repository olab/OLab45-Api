using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_skins")]
    public partial class MapSkins
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [Column("path")]
        [StringLength(200)]
        public string Path { get; set; }
        [Column("user_id", TypeName = "int(10) unsigned")]
        public uint? UserId { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }
        [Column("data", TypeName = "text")]
        public string Data { get; set; }
    }
}
