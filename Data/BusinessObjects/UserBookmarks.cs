using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("user_bookmarks")]
    [Index(nameof(NodeId), Name = "node_id")]
    [Index(nameof(SessionId), Name = "session_id")]
    [Index(nameof(UserId), Name = "user_id")]
    public partial class UserBookmarks
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("session_id", TypeName = "int(10) unsigned")]
        public uint SessionId { get; set; }
        [Column("node_id", TypeName = "int(10) unsigned")]
        public uint NodeId { get; set; }
        [Column("user_id", TypeName = "int(10) unsigned")]
        public uint UserId { get; set; }

        [ForeignKey(nameof(NodeId))]
        [InverseProperty(nameof(MapNodes.UserBookmarks))]
        public virtual MapNodes Node { get; set; }
        [ForeignKey(nameof(SessionId))]
        [InverseProperty(nameof(UserSessions.UserBookmarks))]
        public virtual UserSessions Session { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Users.UserBookmarks))]
        public virtual Users User { get; set; }
    }
}
