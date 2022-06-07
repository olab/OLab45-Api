using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("q_cumulative")]
    [Index(nameof(MapId), Name = "map_id")]
    [Index(nameof(QuestionId), Name = "question_id")]
    public partial class QCumulative
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("question_id", TypeName = "int(10) unsigned")]
        public uint QuestionId { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("reset", TypeName = "int(10)")]
        public int Reset { get; set; }

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.QCumulative))]
        public virtual Maps Map { get; set; }
        [ForeignKey(nameof(QuestionId))]
        [InverseProperty(nameof(MapQuestions.QCumulative))]
        public virtual MapQuestions Question { get; set; }
    }
}
