using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("system_question_responses")]
    [Index(nameof(ParentId), Name = "parent_id")]
    [Index(nameof(QuestionId), Name = "question_id")]
    public partial class SystemQuestionResponses
    {
        public SystemQuestionResponses()
        {
            InverseParent = new HashSet<SystemQuestionResponses>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("name")]
        [StringLength(200)]
        public string Name { get; set; }
        [Column("description", TypeName = "text")]
        public string Description { get; set; }
        [Column("parent_id", TypeName = "int(10) unsigned")]
        public uint? ParentId { get; set; }
        [Column("question_id", TypeName = "int(10) unsigned")]
        public uint? QuestionId { get; set; }
        [Column("response")]
        [StringLength(250)]
        public string Response { get; set; }
        [Column("feedback", TypeName = "text")]
        public string Feedback { get; set; }
        [Column("is_correct")]
        public bool? IsCorrect { get; set; }
        [Column("score", TypeName = "int(10)")]
        public int? Score { get; set; }
        [Column("from")]
        [StringLength(200)]
        public string From { get; set; }
        [Column("to")]
        [StringLength(200)]
        public string To { get; set; }
        [Column("order", TypeName = "int(10) unsigned")]
        public uint Order { get; set; }
        [Column("created_at", TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }
        [Column("updated_At", TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(ParentId))]
        [InverseProperty(nameof(SystemQuestionResponses.InverseParent))]
        public virtual SystemQuestionResponses Parent { get; set; }
        [ForeignKey(nameof(QuestionId))]
        [InverseProperty(nameof(SystemQuestions.SystemQuestionResponses))]
        public virtual SystemQuestions Question { get; set; }
        [InverseProperty(nameof(SystemQuestionResponses.Parent))]
        public virtual ICollection<SystemQuestionResponses> InverseParent { get; set; }
    }
}
