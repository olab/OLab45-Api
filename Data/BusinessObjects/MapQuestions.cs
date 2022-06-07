using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_questions")]
    [Index(nameof(MapId), Name = "map_id")]
    [Index(nameof(ParentId), Name = "parent_id")]
    public partial class MapQuestions
    {
        public MapQuestions()
        {
            InverseParent = new HashSet<MapQuestions>();
            MapQuestionResponses = new HashSet<MapQuestionResponses>();
            MapQuestionValidation = new HashSet<MapQuestionValidation>();
            QCumulative = new HashSet<QCumulative>();
            UserResponses = new HashSet<UserResponses>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("parent_id", TypeName = "int(10) unsigned")]
        public uint? ParentId { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint? MapId { get; set; }
        [Column("stem")]
        [StringLength(500)]
        public string Stem { get; set; }
        [Column("entry_type_id", TypeName = "int(10) unsigned")]
        public uint EntryTypeId { get; set; }
        [Column("width", TypeName = "int(10)")]
        public int Width { get; set; }
        [Column("height", TypeName = "int(10)")]
        public int Height { get; set; }
        [Column("feedback")]
        [StringLength(1000)]
        public string Feedback { get; set; }
        [Required]
        [Column("prompt", TypeName = "text")]
        public string Prompt { get; set; }
        [Column("show_answer")]
        public bool ShowAnswer { get; set; }
        [Column("counter_id", TypeName = "int(10) unsigned")]
        public uint? CounterId { get; set; }
        [Column("num_tries", TypeName = "int(10)")]
        public int NumTries { get; set; }
        [Column("show_submit", TypeName = "tinyint(4)")]
        public sbyte ShowSubmit { get; set; }
        [Column("redirect_node_id", TypeName = "int(10) unsigned")]
        public uint? RedirectNodeId { get; set; }
        [Column("submit_text")]
        [StringLength(200)]
        public string SubmitText { get; set; }
        [Column("type_display", TypeName = "int(10)")]
        public int TypeDisplay { get; set; }
        [Column("settings", TypeName = "text")]
        public string Settings { get; set; }
        [Column("is_private", TypeName = "int(4)")]
        public int IsPrivate { get; set; }
        [Column("order", TypeName = "int(10)")]
        public int? Order { get; set; }
        [Column("external_source_id")]
        [StringLength(255)]
        public string ExternalSourceId { get; set; }

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.MapQuestions))]
        public virtual Maps Map { get; set; }
        [ForeignKey(nameof(ParentId))]
        [InverseProperty(nameof(MapQuestions.InverseParent))]
        public virtual MapQuestions Parent { get; set; }
        [InverseProperty(nameof(MapQuestions.Parent))]
        public virtual ICollection<MapQuestions> InverseParent { get; set; }
        [InverseProperty("Question")]
        public virtual ICollection<MapQuestionResponses> MapQuestionResponses { get; set; }
        [InverseProperty("Question")]
        public virtual ICollection<MapQuestionValidation> MapQuestionValidation { get; set; }
        [InverseProperty("Question")]
        public virtual ICollection<QCumulative> QCumulative { get; set; }
        [InverseProperty("Question")]
        public virtual ICollection<UserResponses> UserResponses { get; set; }
    }
}
