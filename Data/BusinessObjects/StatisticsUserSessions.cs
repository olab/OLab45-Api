using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("statistics_user_sessions")]
    public partial class StatisticsUserSessions
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("user_id", TypeName = "int(10) unsigned")]
        public uint UserId { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("start_time")]
        public decimal StartTime { get; set; }
        [Column("end_time")]
        public decimal? EndTime { get; set; }
        [Required]
        [Column("user_ip")]
        [StringLength(50)]
        public string UserIp { get; set; }
        [Column("webinar_id", TypeName = "int(10) unsigned")]
        public uint WebinarId { get; set; }
        [Column("webinar_step", TypeName = "int(10)")]
        public int WebinarStep { get; set; }
        [Column("date_save_id", TypeName = "int(10) unsigned")]
        public uint DateSaveId { get; set; }
    }
}
