using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_popups_styles")]
    public partial class MapPopupsStyles
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("map_popup_id", TypeName = "int(10) unsigned")]
        public uint MapPopupId { get; set; }
        [Column("is_default_background_color", TypeName = "tinyint(4)")]
        public sbyte IsDefaultBackgroundColor { get; set; }
        [Column("is_background_transparent", TypeName = "tinyint(4)")]
        public sbyte IsBackgroundTransparent { get; set; }
        [Column("background_color")]
        [StringLength(10)]
        public string BackgroundColor { get; set; }
        [Column("font_color")]
        [StringLength(10)]
        public string FontColor { get; set; }
        [Column("border_color")]
        [StringLength(10)]
        public string BorderColor { get; set; }
        [Column("is_border_transparent", TypeName = "tinyint(4)")]
        public sbyte IsBorderTransparent { get; set; }
        [Required]
        [Column("background_transparent")]
        [StringLength(4)]
        public string BackgroundTransparent { get; set; }
        [Required]
        [Column("border_transparent")]
        [StringLength(4)]
        public string BorderTransparent { get; set; }
    }
}
