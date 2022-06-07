using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("scenarios")]
    public partial class Scenarios
    {
        public Scenarios()
        {
            ScenarioMaps = new HashSet<ScenarioMaps>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Required]
        [Column("name")]
        [StringLength(45)]
        public string Name { get; set; }
        [Column("description")]
        [StringLength(100)]
        public string Description { get; set; }

        [InverseProperty("Scenario")]
        public virtual ICollection<ScenarioMaps> ScenarioMaps { get; set; }
    }
}
