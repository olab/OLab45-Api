using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("statistics_user_responses")]
    public partial class StatisticsUserResponses
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("question_id", TypeName = "int(10) unsigned")]
        public uint QuestionId { get; set; }
        [Column("session_id", TypeName = "int(10) unsigned")]
        public uint SessionId { get; set; }
        [Required]
        [Column("response")]
        [StringLength(700)]
        public string Response { get; set; }
        [Column("node_id", TypeName = "int(10) unsigned")]
        public uint NodeId { get; set; }
    }
}
