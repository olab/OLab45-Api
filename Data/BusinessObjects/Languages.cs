using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("languages")]
    [Index(nameof(Name), Name = "name")]
    public partial class Languages
    {
        public Languages()
        {
            Maps = new HashSet<Maps>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Required]
        [Column("name")]
        [StringLength(20)]
        public string Name { get; set; }
        [Required]
        [Column("key")]
        [StringLength(20)]
        public string Key { get; set; }

        [InverseProperty("Language")]
        public virtual ICollection<Maps> Maps { get; set; }
    }
}
