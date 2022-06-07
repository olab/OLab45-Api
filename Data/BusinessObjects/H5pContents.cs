using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("h5p_contents")]
    public partial class H5pContents
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("created_at", TypeName = "timestamp")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at", TypeName = "timestamp")]
        public DateTime UpdatedAt { get; set; }
        [Column("user_id", TypeName = "int(10) unsigned")]
        public uint UserId { get; set; }
        [Required]
        [Column("title")]
        [StringLength(255)]
        public string Title { get; set; }
        [Column("library_id", TypeName = "int(10) unsigned")]
        public uint LibraryId { get; set; }
        [Required]
        [Column("parameters")]
        public string Parameters { get; set; }
        [Required]
        [Column("filtered")]
        public string Filtered { get; set; }
        [Required]
        [Column("slug")]
        [StringLength(127)]
        public string Slug { get; set; }
        [Required]
        [Column("embed_type")]
        [StringLength(127)]
        public string EmbedType { get; set; }
        [Column("disable", TypeName = "int(10) unsigned")]
        public uint Disable { get; set; }
        [Column("content_type")]
        [StringLength(127)]
        public string ContentType { get; set; }
        [Column("author")]
        [StringLength(127)]
        public string Author { get; set; }
        [Column("license")]
        [StringLength(7)]
        public string License { get; set; }
        [Column("keywords", TypeName = "text")]
        public string Keywords { get; set; }
        [Column("description", TypeName = "text")]
        public string Description { get; set; }
    }
}
