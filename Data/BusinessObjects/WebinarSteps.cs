using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("webinar_steps")]
    public partial class WebinarSteps
    {
        public WebinarSteps()
        {
            WebinarMaps = new HashSet<WebinarMaps>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("webinar_id", TypeName = "int(10) unsigned")]
        public uint WebinarId { get; set; }
        [Required]
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }

        [InverseProperty("StepNavigation")]
        public virtual ICollection<WebinarMaps> WebinarMaps { get; set; }
    }
}
