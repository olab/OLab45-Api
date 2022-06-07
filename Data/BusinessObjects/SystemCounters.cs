using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("system_counters")]
    public partial class SystemCounters
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("name")]
        [StringLength(200)]
        public string Name { get; set; }
        [Column("description", TypeName = "text")]
        public string Description { get; set; }
        [Column("start_value", TypeName = "blob")]
        public byte[] StartValue { get; set; }
        [Column("value", TypeName = "blob")]
        public byte[] Value { get; set; }
        [Column("icon_id", TypeName = "int(10)")]
        public int? IconId { get; set; }
        [Column("prefix")]
        [StringLength(20)]
        public string Prefix { get; set; }
        [Column("suffix")]
        [StringLength(20)]
        public string Suffix { get; set; }
        [Column("visible")]
        public bool? Visible { get; set; }
        [Column("out_of", TypeName = "int(10)")]
        public int? OutOf { get; set; }
        [Column("status", TypeName = "int(1)")]
        public int Status { get; set; }
        [Column("imageable_id", TypeName = "int(10) unsigned")]
        public uint ImageableId { get; set; }
        [Required]
        [Column("imageable_type")]
        [StringLength(45)]
        public string ImageableType { get; set; }
        [Column("is_system", TypeName = "int(10)")]
        public int? IsSystem { get; set; }
        [Column("created_at", TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_At", TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        [InverseProperty("Counter")]
        public virtual ICollection<SystemCounterActions> SystemCounterActions { get; set; }
    }
}
