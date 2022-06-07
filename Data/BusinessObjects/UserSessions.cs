using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("user_sessions")]
    [Index(nameof(Id), Name = "id", IsUnique = true)]
    [Index(nameof(MapId), Name = "map_id")]
    [Index(nameof(UserId), Name = "user_id")]
    [Index(nameof(Uuid), Name = "uuid", IsUnique = true)]
    public partial class UserSessions
    {
        public UserSessions()
        {
            Statements = new HashSet<Statements>();
            UserBookmarks = new HashSet<UserBookmarks>();
            UserResponses = new HashSet<UserResponses>();
            UserSessionTraces = new HashSet<UserSessionTraces>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Required]
        [Column("uuid")]
        public string Uuid { get; set; }
        [Column("user_id", TypeName = "int(10) unsigned")]
        public uint UserId { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("start_time")]
        public decimal StartTime { get; set; }
        [Required]
        [Column("user_ip")]
        [StringLength(50)]
        public string UserIp { get; set; }
        [Column("webinar_id", TypeName = "int(10)")]
        public int? WebinarId { get; set; }
        [Column("webinar_step", TypeName = "int(10)")]
        public int? WebinarStep { get; set; }
        [Column("notCumulative")]
        public bool NotCumulative { get; set; }
        [Column("reset_at")]
        public decimal? ResetAt { get; set; }
        [Column("end_time")]
        public decimal? EndTime { get; set; }

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.UserSessions))]
        public virtual Maps Map { get; set; }
        [InverseProperty("Session")]
        public virtual UserNotes UserNotes { get; set; }
        [InverseProperty("Session")]
        public virtual ICollection<Statements> Statements { get; set; }
        [InverseProperty("Session")]
        public virtual ICollection<UserBookmarks> UserBookmarks { get; set; }
        [InverseProperty("Session")]
        public virtual ICollection<UserResponses> UserResponses { get; set; }
        [InverseProperty("Session")]
        public virtual ICollection<UserSessionTraces> UserSessionTraces { get; set; }
    }
}
