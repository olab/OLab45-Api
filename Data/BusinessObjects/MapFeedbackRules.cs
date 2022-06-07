using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_feedback_rules")]
    [Index(nameof(MapId), Name = "map_id")]
    public partial class MapFeedbackRules
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("rule_type_id", TypeName = "int(10) unsigned")]
        public uint RuleTypeId { get; set; }
        [Column("value", TypeName = "int(10)")]
        public int? Value { get; set; }
        [Column("operator_id", TypeName = "int(10) unsigned")]
        public uint? OperatorId { get; set; }
        [Column("message", TypeName = "text")]
        public string Message { get; set; }
        [Column("counter_id", TypeName = "int(10) unsigned")]
        public uint? CounterId { get; set; }

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.MapFeedbackRules))]
        public virtual Maps Map { get; set; }
    }
}
