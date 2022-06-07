using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("user_state")]
    [Index(nameof(MapId), Name = "map_fk_idx")]
    [Index(nameof(MapNodeId), Name = "map_node_fk_idx")]
    [Index(nameof(UserId), Name = "user_fk_idx")]
    public partial class UserState
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("user_id", TypeName = "int(10) unsigned")]
        public uint UserId { get; set; }
        [Column("map_id", TypeName = "int(10) unsigned")]
        public uint MapId { get; set; }
        [Column("map_node_id", TypeName = "int(10) unsigned")]
        public uint MapNodeId { get; set; }
        [Required]
        [Column("state_data", TypeName = "blob")]
        public byte[] StateData { get; set; }
        [Column("created_at", TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }
        [Column("updated_at", TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }
        [Column("session_id", TypeName = "int(10) unsigned")]
        public uint SessionId { get; set; }

        [ForeignKey(nameof(MapId))]
        [InverseProperty(nameof(Maps.UserState))]
        public virtual Maps Map { get; set; }
        [ForeignKey(nameof(MapNodeId))]
        [InverseProperty(nameof(MapNodes.UserState))]
        public virtual MapNodes MapNode { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Users.UserState))]
        public virtual Users User { get; set; }
    }
}
