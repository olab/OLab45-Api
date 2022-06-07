using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("system_constants")]
    public partial class SystemConstants
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("name")]
        [StringLength(45)]
        public string Name { get; set; }
        [Column("description", TypeName = "text")]
        public string Description { get; set; }
        [Column("imageable_id", TypeName = "int(10) unsigned")]
        public uint ImageableId { get; set; }
        [Required]
        [Column("imageable_type")]
        [StringLength(45)]
        public string ImageableType { get; set; }
        [Column("value", TypeName = "blob")]
        public byte[] Value { get; set; }
        [Column("is_system", TypeName = "int(10)")]
        public int? IsSystem { get; set; }
        [Column("created_at", TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }
        [Column("updated_At", TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }
    }
}
