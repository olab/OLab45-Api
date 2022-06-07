using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("groups")]
    [Index(nameof(Name), Name = "name")]
    public partial class Groups
    {
        public Groups()
        {
            UserGroups = new HashSet<UserGroups>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; }

        [InverseProperty("Group")]
        public virtual ICollection<UserGroups> UserGroups { get; set; }
    }
}
