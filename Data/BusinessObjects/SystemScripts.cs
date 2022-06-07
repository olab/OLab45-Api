using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("system_scripts")]
    public partial class SystemScripts
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("name")]
        [StringLength(45)]
        public string Name { get; set; }
        [Column("description", TypeName = "text")]
        public string Description { get; set; }
        [Column("source", TypeName = "blob")]
        public byte[] Source { get; set; }
        [Column("is_raw", TypeName = "bit(1)")]
        public ulong? IsRaw { get; set; }
        [Column("order", TypeName = "int(10)")]
        public int? Order { get; set; }
        [Column("postload_id", TypeName = "int(10)")]
        public int? PostloadId { get; set; }
        [Required]
        [Column("imageable_type")]
        [StringLength(45)]
        public string ImageableType { get; set; }
        [Column("imageable_id", TypeName = "int(10) unsigned")]
        public uint ImageableId { get; set; }
        [Column("system_scriptscol")]
        [StringLength(45)]
        public string SystemScriptscol { get; set; }
        [Column("created_at", TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }
        [Column("updated_At", TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }
    }
}
