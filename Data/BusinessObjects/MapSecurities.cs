using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_securities")]
    public partial class MapSecurities
    {
        public MapSecurities()
        {
            Maps = new HashSet<Maps>();
        }

        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [Column("description")]
        [StringLength(700)]
        public string Description { get; set; }

        [InverseProperty("Security")]
        public virtual ICollection<Maps> Maps { get; set; }
    }
}
