using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_dam_elements")]
    [Index(nameof(DamId), Name = "dam_id")]
    [Index(nameof(ElementId), Name = "element_id")]
    public partial class MapDamElements
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("dam_id", TypeName = "int(10) unsigned")]
        public uint DamId { get; set; }
        [Column("element_type")]
        [StringLength(20)]
        public string ElementType { get; set; }
        [Column("order", TypeName = "int(10)")]
        public int? Order { get; set; }
        [Required]
        [Column("display")]
        [StringLength(20)]
        public string Display { get; set; }
        [Column("element_id", TypeName = "int(10) unsigned")]
        public uint ElementId { get; set; }

        [ForeignKey(nameof(DamId))]
        [InverseProperty(nameof(MapDams.MapDamElements))]
        public virtual MapDams Dam { get; set; }
    }
}
