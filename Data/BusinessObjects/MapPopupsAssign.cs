using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_popups_assign")]
    public partial class MapPopupsAssign
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_popup_id", TypeName = "int(10) unsigned")]
        public uint MapPopupId { get; set; }
        [Column("assign_type_id", TypeName = "int(10) unsigned")]
        public uint AssignTypeId { get; set; }
        [Column("assign_to_id", TypeName = "int(10) unsigned")]
        public uint AssignToId { get; set; }
        [Column("redirect_to_id", TypeName = "int(10) unsigned")]
        public uint? RedirectToId { get; set; }
        [Column("redirect_type_id", TypeName = "int(10) unsigned")]
        public uint RedirectTypeId { get; set; }
    }
}
