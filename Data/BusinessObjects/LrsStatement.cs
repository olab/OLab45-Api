using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("lrs_statement")]
    [Index(nameof(LrsId), Name = "lrs_id")]
    [Index(nameof(StatementId), Name = "statement_id")]
    public partial class LrsStatement
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("lrs_id", TypeName = "int(10) unsigned")]
        public uint LrsId { get; set; }
        [Column("statement_id", TypeName = "int(10) unsigned")]
        public uint StatementId { get; set; }
        [Column("status", TypeName = "tinyint(3) unsigned")]
        public byte Status { get; set; }
        [Column("created_at", TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at", TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(LrsId))]
        [InverseProperty("LrsStatement")]
        public virtual Lrs Lrs { get; set; }
        [ForeignKey(nameof(StatementId))]
        [InverseProperty(nameof(Statements.LrsStatement))]
        public virtual Statements Statement { get; set; }
    }
}
