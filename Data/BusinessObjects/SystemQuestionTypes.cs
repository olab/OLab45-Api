using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("system_question_types")]
    public partial class SystemQuestionTypes
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("title")]
        [StringLength(70)]
        public string Title { get; set; }
        [Column("value")]
        [StringLength(20)]
        public string Value { get; set; }
        [Required]
        [Column("template_name")]
        [StringLength(200)]
        public string TemplateName { get; set; }
        [Column("template_args")]
        [StringLength(100)]
        public string TemplateArgs { get; set; }
    }
}
