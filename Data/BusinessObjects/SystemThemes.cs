using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("system_themes")]
    public partial class SystemThemes
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("name")]
        [StringLength(45)]
        public string Name { get; set; }
        [Column("description", TypeName = "text")]
        public string Description { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint? MapId { get; set; }
        [Column("header_text", TypeName = "text")]
        public string HeaderText { get; set; }
        [Column("footer_text", TypeName = "text")]
        public string FooterText { get; set; }
        [Column("left_text", TypeName = "text")]
        public string LeftText { get; set; }
        [Column("right_text", TypeName = "text")]
        public string RightText { get; set; }
        [Required]
        [Column("imageable_type")]
        [StringLength(45)]
        public string ImageableType { get; set; }
        [Column("imageable_id", TypeName = "int(10) unsigned")]
        public uint ImageableId { get; set; }
        [Column("created_at", TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }
        [Column("updated_At", TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }
    }
}
