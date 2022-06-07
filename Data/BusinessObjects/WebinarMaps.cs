using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("webinar_maps")]
    [Index(nameof(Step), Name = "step")]
    public partial class WebinarMaps
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("webinar_id", TypeName = "int(10) unsigned")]
        public uint WebinarId { get; set; }
        [Required]
        [Column("which", TypeName = "enum('labyrinth','section')")]
        public string Which { get; set; }
        [Column("reference_id", TypeName = "int(10) unsigned")]
        public uint ReferenceId { get; set; }
        [Column("step", TypeName = "int(10) unsigned")]
        public uint Step { get; set; }
        [Column("cumulative")]
        public bool Cumulative { get; set; }

        [ForeignKey(nameof(Step))]
        [InverseProperty(nameof(WebinarSteps.WebinarMaps))]
        public virtual WebinarSteps StepNavigation { get; set; }
    }
}
