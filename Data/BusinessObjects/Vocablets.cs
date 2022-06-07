using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("vocablets")]
    [Index(nameof(Guid), Name = "guid", IsUnique = true)]
    public partial class Vocablets
    {
        [Required]
        [Column("guid")]
        [StringLength(50)]
        public string Guid { get; set; }
        [Required]
        [Column("state")]
        [StringLength(10)]
        public string State { get; set; }
        [Required]
        [Column("version")]
        [StringLength(5)]
        public string Version { get; set; }
        [Required]
        [Column("name")]
        [StringLength(64)]
        public string Name { get; set; }
        [Required]
        [Column("path")]
        [StringLength(128)]
        public string Path { get; set; }
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
    }
}
