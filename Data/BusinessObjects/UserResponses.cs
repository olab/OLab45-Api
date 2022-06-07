using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("user_responses")]
    [Index(nameof(QuestionId), Name = "question_id")]
    [Index(nameof(SessionId), Name = "session_id")]
    public partial class UserResponses
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("question_id", TypeName = "int(10) unsigned")]
        public uint QuestionId { get; set; }
        [Column("session_id", TypeName = "int(10) unsigned")]
        public uint SessionId { get; set; }
        [Column("response")]
        [StringLength(1000)]
        public string Response { get; set; }
        [Column("node_id", TypeName = "int(10) unsigned")]
        public uint NodeId { get; set; }
        [Column("created_at")]
        public decimal CreatedAt { get; set; }

        [ForeignKey(nameof(QuestionId))]
        [InverseProperty(nameof(MapQuestions.UserResponses))]
        public virtual MapQuestions Question { get; set; }
        [ForeignKey(nameof(SessionId))]
        [InverseProperty(nameof(UserSessions.UserResponses))]
        public virtual UserSessions Session { get; set; }
    }
}
