using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_question_responses")]
    [Index(nameof(ParentId), Name = "parent_id")]
    [Index(nameof(QuestionId), Name = "question_id")]
    public partial class MapQuestionResponses
    {
        public MapQuestionResponses()
        {
            InverseParent = new HashSet<MapQuestionResponses>();
            SjtResponse = new HashSet<SjtResponse>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
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
        public bool IsCorrect { get; set; }
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

        [ForeignKey(nameof(ParentId))]
        [InverseProperty(nameof(MapQuestionResponses.InverseParent))]
        public virtual MapQuestionResponses Parent { get; set; }
        [ForeignKey(nameof(QuestionId))]
        [InverseProperty(nameof(MapQuestions.MapQuestionResponses))]
        public virtual MapQuestions Question { get; set; }
        [InverseProperty(nameof(MapQuestionResponses.Parent))]
        public virtual ICollection<MapQuestionResponses> InverseParent { get; set; }
        [InverseProperty("Response")]
        public virtual ICollection<SjtResponse> SjtResponse { get; set; }
    }
}
