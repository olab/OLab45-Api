using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_counter_rules")]
    [Index(nameof(CounterId), Name = "counter_id")]
    public partial class MapCounterRules
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("counter_id", TypeName = "int(10) unsigned")]
        public uint CounterId { get; set; }
        [Column("relation_id", TypeName = "int(10) unsigned")]
        public uint RelationId { get; set; }
        [Column("value")]
        public double Value { get; set; }
        [Column("function")]
        [StringLength(50)]
        public string Function { get; set; }
        [Column("redirect_node_id", TypeName = "int(10)")]
        public int? RedirectNodeId { get; set; }
        [Column("message")]
        [StringLength(500)]
        public string Message { get; set; }
        [Column("counter", TypeName = "int(10)")]
        public int? Counter { get; set; }
        [Column("counter_value")]
        [StringLength(50)]
        public string CounterValue { get; set; }

        [ForeignKey(nameof(CounterId))]
        [InverseProperty(nameof(MapCounters.MapCounterRules))]
        public virtual MapCounters CounterNavigation { get; set; }
    }
}
