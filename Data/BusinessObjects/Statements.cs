using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("statements")]
    [Index(nameof(SessionId), Name = "session_id")]
    public partial class Statements
    {
        public Statements()
        {
            LrsStatement = new HashSet<LrsStatement>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("session_id", TypeName = "int(10) unsigned")]
        public uint? SessionId { get; set; }
        [Column("initiator", TypeName = "tinyint(3) unsigned")]
        public byte Initiator { get; set; }
        [Required]
        [Column("statement", TypeName = "text")]
        public string Statement { get; set; }
        [Column("timestamp")]
        public decimal Timestamp { get; set; }
        [Column("created_at", TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at", TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(SessionId))]
        [InverseProperty(nameof(UserSessions.Statements))]
        public virtual UserSessions Session { get; set; }
        [InverseProperty("Statement")]
        public virtual ICollection<LrsStatement> LrsStatement { get; set; }
    }
}
