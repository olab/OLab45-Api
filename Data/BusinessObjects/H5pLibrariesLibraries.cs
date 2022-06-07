using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("h5p_libraries_libraries")]
    public partial class H5pLibrariesLibraries
    {
        [Key]
        [Column("library_id", TypeName = "int(10) unsigned")]
        public uint LibraryId { get; set; }
        [Key]
        [Column("required_library_id", TypeName = "int(10) unsigned")]
        public uint RequiredLibraryId { get; set; }
        [Required]
        [Column("dependency_type")]
        [StringLength(31)]
        public string DependencyType { get; set; }
    }
}
