using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("phinxlog")]
    public partial class Phinxlog
    {
        [Key]
        [Column("version", TypeName = "bigint(20)")]
        public long Version { get; set; }
        [Column("migration_name")]
        [StringLength(100)]
        public string MigrationName { get; set; }
        [Column("start_time", TypeName = "timestamp")]
        public DateTime StartTime { get; set; }
        [Column("end_time", TypeName = "timestamp")]
        public DateTime EndTime { get; set; }
        [Column("breakpoint")]
        public bool Breakpoint { get; set; }
    }
}
