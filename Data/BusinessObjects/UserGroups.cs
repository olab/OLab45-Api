using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("user_groups")]
    [Index(nameof(GroupId), Name = "group_id")]
    [Index(nameof(UserId), Name = "user_id")]
    public partial class UserGroups
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("user_id", TypeName = "int(10) unsigned")]
        public uint UserId { get; set; }
        [Column("group_id", TypeName = "int(10) unsigned")]
        public uint GroupId { get; set; }

        [ForeignKey(nameof(GroupId))]
        [InverseProperty(nameof(Groups.UserGroups))]
        public virtual Groups Group { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Users.UserGroups))]
        public virtual Users User { get; set; }
    }
}
