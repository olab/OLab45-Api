using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("scenario_maps")]
    [Index(nameof(MapId), Name = "fk_scenario_maps_maps_idx")]
    [Index(nameof(ScenarioId), Name = "fk_scenerio_maps_idx")]
    public partial class ScenarioMaps
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("scenario_id", TypeName = "int(10) unsigned")]
        public uint ScenarioId { get; set; }

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.ScenarioMaps))]
        public virtual Maps Map { get; set; }
        [ForeignKey(nameof(ScenarioId))]
        [InverseProperty(nameof(Scenarios.ScenarioMaps))]
        public virtual Scenarios Scenario { get; set; }
    }
}
