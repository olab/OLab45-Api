using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("webinar_node_poll")]
    [Index(nameof(NodeId), Name = "node_id")]
    [Index(nameof(WebinarId), Name = "webinar_id")]
    public partial class WebinarNodePoll
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("node_id", TypeName = "int(10) unsigned")]
        public uint NodeId { get; set; }
        [Column("webinar_id", TypeName = "int(10) unsigned")]
        public uint WebinarId { get; set; }
        [Column("time", TypeName = "int(10)")]
        public int Time { get; set; }

        [ForeignKey(nameof(NodeId))]
        [InverseProperty(nameof(MapNodes.WebinarNodePoll))]
        public virtual MapNodes Node { get; set; }
        [ForeignKey(nameof(WebinarId))]
        [InverseProperty(nameof(Webinars.WebinarNodePoll))]
        public virtual Webinars Webinar { get; set; }
    }
}
