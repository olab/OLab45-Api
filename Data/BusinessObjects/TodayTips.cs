using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("today_tips")]
    public partial class TodayTips
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Required]
        [Column("title")]
        [StringLength(300)]
        public string Title { get; set; }
        [Required]
        [Column("text", TypeName = "text")]
        public string Text { get; set; }
        [Column("start_date", TypeName = "datetime")]
        public DateTime StartDate { get; set; }
        [Column("weight", TypeName = "int(10)")]
        public int Weight { get; set; }
        [Column("is_active", TypeName = "tinyint(4)")]
        public sbyte IsActive { get; set; }
        [Column("is_archived", TypeName = "tinyint(4)")]
        public sbyte IsArchived { get; set; }
        [Column("end_date", TypeName = "datetime")]
        public DateTime? EndDate { get; set; }
    }
}
