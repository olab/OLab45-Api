using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_popups_counters")]
    [Index(nameof(CounterId), Name = "counter_id")]
    [Index(nameof(PopupId), Name = "popup_id")]
    public partial class MapPopupsCounters
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("popup_id", TypeName = "int(10) unsigned")]
        public uint PopupId { get; set; }
        [Column("counter_id", TypeName = "int(10) unsigned")]
        public uint CounterId { get; set; }
        [Required]
        [Column("function", TypeName = "text")]
        public string Function { get; set; }

        [ForeignKey(nameof(PopupId))]
        [InverseProperty(nameof(MapPopups.MapPopupsCounters))]
        public virtual MapPopups Popup { get; set; }
    }
}
