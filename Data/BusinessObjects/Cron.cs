using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("cron")]
    [Index(nameof(RuleId), Name = "rule_id")]
    public partial class Cron
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("rule_id", TypeName = "int(10) unsigned")]
        public uint RuleId { get; set; }
        [Column("activate", TypeName = "int(10)")]
        public int? Activate { get; set; }

        [ForeignKey(nameof(RuleId))]
        [InverseProperty(nameof(MapCounterCommonRules.Cron))]
        public virtual MapCounterCommonRules Rule { get; set; }
    }
}
