using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("user_sessiontraces")]
    [Index(nameof(MapId), Name = "map_id")]
    [Index(nameof(NodeId), Name = "node_id")]
    [Index(nameof(SessionId), Name = "session_id")]
    [Index(nameof(UserId), Name = "user_id")]
    public partial class UserSessionTraces
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
        [StringLength(2000)]
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
        [Column("end_date_stamp")]
        public decimal? EndDateStamp { get; set; }

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.UserSessionTraces))]
        public virtual Maps Map { get; set; }
        [ForeignKey(nameof(NodeId))]
        [InverseProperty(nameof(MapNodes.UserSessionTraces))]
        public virtual MapNodes Node { get; set; }
        [ForeignKey(nameof(SessionId))]
        [InverseProperty(nameof(UserSessions.UserSessionTraces))]
        public virtual UserSessions Session { get; set; }
    }
}
