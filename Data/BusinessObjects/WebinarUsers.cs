using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("webinar_users")]
    [Index(nameof(UserId), Name = "user_id")]
    public partial class WebinarUsers
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("webinar_id", TypeName = "int(10) unsigned")]
        public uint WebinarId { get; set; }
        [Column("user_id", TypeName = "int(10) unsigned")]
        public uint UserId { get; set; }
        [Column("include_4R")]
        public bool Include4r { get; set; }
        [Column("expert")]
        public bool Expert { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Users.WebinarUsers))]
        public virtual Users User { get; set; }
    }
}
