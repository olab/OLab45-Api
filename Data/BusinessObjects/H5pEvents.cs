using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("h5p_events")]
    public partial class H5pEvents
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("user_id", TypeName = "int(10) unsigned")]
        public uint UserId { get; set; }
        [Column("created_at", TypeName = "int(10) unsigned")]
        public uint CreatedAt { get; set; }
        [Required]
        [Column("type")]
        [StringLength(63)]
        public string Type { get; set; }
        [Required]
        [Column("sub_type")]
        [StringLength(63)]
        public string SubType { get; set; }
        [Column("content_id", TypeName = "int(10) unsigned")]
        public uint ContentId { get; set; }
        [Required]
        [Column("content_title")]
        [StringLength(255)]
        public string ContentTitle { get; set; }
        [Required]
        [Column("library_name")]
        [StringLength(127)]
        public string LibraryName { get; set; }
        [Required]
        [Column("library_version")]
        [StringLength(31)]
        public string LibraryVersion { get; set; }
    }
}
