using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("h5p_libraries")]
    [Index(nameof(Name), nameof(MajorVersion), nameof(MinorVersion), nameof(PatchVersion), Name = "name_version")]
    [Index(nameof(Runnable), Name = "runnable")]
    public partial class H5pLibraries
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("created_at", TypeName = "timestamp")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at", TypeName = "timestamp")]
        public DateTime UpdatedAt { get; set; }
        [Required]
        [Column("name")]
        [StringLength(127)]
        public string Name { get; set; }
        [Required]
        [Column("title")]
        [StringLength(255)]
        public string Title { get; set; }
        [Column("major_version", TypeName = "int(10) unsigned")]
        public uint MajorVersion { get; set; }
        [Column("minor_version", TypeName = "int(10) unsigned")]
        public uint MinorVersion { get; set; }
        [Column("patch_version", TypeName = "int(10) unsigned")]
        public uint PatchVersion { get; set; }
        [Column("runnable", TypeName = "int(10) unsigned")]
        public uint Runnable { get; set; }
        [Column("restricted", TypeName = "int(10) unsigned")]
        public uint Restricted { get; set; }
        [Column("fullscreen", TypeName = "int(10) unsigned")]
        public uint Fullscreen { get; set; }
        [Required]
        [Column("embed_types")]
        [StringLength(255)]
        public string EmbedTypes { get; set; }
        [Column("preloaded_js", TypeName = "text")]
        public string PreloadedJs { get; set; }
        [Column("preloaded_css", TypeName = "text")]
        public string PreloadedCss { get; set; }
        [Column("drop_library_css", TypeName = "text")]
        public string DropLibraryCss { get; set; }
        [Required]
        [Column("semantics", TypeName = "text")]
        public string Semantics { get; set; }
        [Required]
        [Column("tutorial_url")]
        [StringLength(1023)]
        public string TutorialUrl { get; set; }
    }
}
