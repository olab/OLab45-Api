using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("statistics_user_sessiontraces")]
    public partial class StatisticsUserSessionTraces
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("session_id", TypeName = "int(10) unsigned")]
        public uint SessionId { get; set; }
        [Column("user_id", TypeName = "int(10) unsigned")]
        public uint UserId { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("node_id", TypeName = "int(10) unsigned")]
        public uint NodeId { get; set; }
        [Column("is_redirected")]
        public bool IsRedirected { get; set; }
        [Column("counters")]
        [StringLength(700)]
        public string Counters { get; set; }
        [Column("date_stamp")]
        public decimal? DateStamp { get; set; }
        [Column("confidence", TypeName = "smallint(6)")]
        public short? Confidence { get; set; }
        [Column("dams")]
        [StringLength(700)]
        public string Dams { get; set; }
        [Column("bookmark_made")]
        public decimal? BookmarkMade { get; set; }
        [Column("bookmark_used")]
        public decimal? BookmarkUsed { get; set; }
        [Column("end_date_stamp", TypeName = "decimal(18,6) unsigned")]
        public decimal? EndDateStamp { get; set; }
    }
}
