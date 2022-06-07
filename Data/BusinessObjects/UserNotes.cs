using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("user_notes")]
    [Index(nameof(SessionId), Name = "session_id", IsUnique = true)]
    [Index(nameof(UserId), Name = "user_id")]
    [Index(nameof(WebinarId), Name = "webinar_id")]
    public partial class UserNotes
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("user_id", TypeName = "int(10) unsigned")]
        public uint? UserId { get; set; }
        [Column("session_id", TypeName = "int(10) unsigned")]
        public uint? SessionId { get; set; }
        [Column("webinar_id", TypeName = "int(10) unsigned")]
        public uint? WebinarId { get; set; }
        [Required]
        [Column("text", TypeName = "text")]
        public string Text { get; set; }
        [Column("created_at")]
        public decimal CreatedAt { get; set; }
        [Column("updated_at")]
        public decimal UpdatedAt { get; set; }
        [Column("deleted_at")]
        public decimal? DeletedAt { get; set; }

        [ForeignKey(nameof(SessionId))]
        [InverseProperty(nameof(UserSessions.UserNotes))]
        public virtual UserSessions Session { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Users.UserNotes))]
        public virtual Users User { get; set; }
        [ForeignKey(nameof(WebinarId))]
        [InverseProperty(nameof(Webinars.UserNotes))]
        public virtual Webinars Webinar { get; set; }
    }
}
