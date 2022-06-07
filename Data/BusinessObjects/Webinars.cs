using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("webinars")]
    public partial class Webinars
    {
        public Webinars()
        {
            UserNotes = new HashSet<UserNotes>();
            WebinarNodePoll = new HashSet<WebinarNodePoll>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Required]
        [Column("title")]
        [StringLength(250)]
        public string Title { get; set; }
        [Column("current_step", TypeName = "int(10)")]
        public int? CurrentStep { get; set; }
        [Column("forum_id", TypeName = "int(10) unsigned")]
        public uint ForumId { get; set; }
        [Column("isForum")]
        public bool IsForum { get; set; }
        [Column("publish")]
        [StringLength(100)]
        public string Publish { get; set; }
        [Column("author_id", TypeName = "int(10) unsigned")]
        public uint AuthorId { get; set; }
        [Required]
        [Column("changeSteps", TypeName = "enum('manually','automatic')")]
        public string ChangeSteps { get; set; }

        [InverseProperty("Webinar")]
        public virtual ICollection<UserNotes> UserNotes { get; set; }
        [InverseProperty("Webinar")]
        public virtual ICollection<WebinarNodePoll> WebinarNodePoll { get; set; }
    }
}
