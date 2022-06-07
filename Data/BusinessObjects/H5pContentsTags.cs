using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("h5p_contents_tags")]
    public partial class H5pContentsTags
    {
        [Key]
        [Column("content_id", TypeName = "int(10) unsigned")]
        public uint ContentId { get; set; }
        [Key]
        [Column("tag_id", TypeName = "int(10) unsigned")]
        public uint TagId { get; set; }
    }
}
