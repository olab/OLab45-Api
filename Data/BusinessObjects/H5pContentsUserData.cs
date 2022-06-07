using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("h5p_contents_user_data")]
    public partial class H5pContentsUserData
    {
        [Key]
        [Column("content_id", TypeName = "int(10) unsigned")]
        public uint ContentId { get; set; }
        [Key]
        [Column("user_id", TypeName = "int(10) unsigned")]
        public uint UserId { get; set; }
        [Key]
        [Column("sub_content_id", TypeName = "int(10) unsigned")]
        public uint SubContentId { get; set; }
        [Key]
        [Column("data_id")]
        [StringLength(127)]
        public string DataId { get; set; }
        [Required]
        [Column("data")]
        public string Data { get; set; }
        [Column("preload", TypeName = "tinyint(3) unsigned")]
        public byte Preload { get; set; }
        [Column("invalidate", TypeName = "tinyint(3) unsigned")]
        public byte Invalidate { get; set; }
        [Column("updated_at", TypeName = "timestamp")]
        public DateTime UpdatedAt { get; set; }
    }
}
