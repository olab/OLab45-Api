using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("webinar_poll")]
    [Index(nameof(OnNode), Name = "on_node")]
    [Index(nameof(ToNode), Name = "to_node")]
    public partial class WebinarPoll
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("on_node", TypeName = "int(10) unsigned")]
        public uint OnNode { get; set; }
        [Column("to_node", TypeName = "int(10) unsigned")]
        public uint ToNode { get; set; }
        [Column("time", TypeName = "int(10)")]
        public int Time { get; set; }

        [ForeignKey(nameof(OnNode))]
        [InverseProperty(nameof(MapNodes.WebinarPollOnNodeNavigation))]
        public virtual MapNodes OnNodeNavigation { get; set; }
        [ForeignKey(nameof(ToNode))]
        [InverseProperty(nameof(MapNodes.WebinarPollToNodeNavigation))]
        public virtual MapNodes ToNodeNavigation { get; set; }
    }
}
