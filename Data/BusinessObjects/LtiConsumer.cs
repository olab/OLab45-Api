using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("lti_consumer")]
    [Index(nameof(ConsumerKey), Name = "consumer_key")]
    public partial class LtiConsumer
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Required]
        [Column("consumer_key")]
        public string ConsumerKey { get; set; }
        [Required]
        [Column("name")]
        [StringLength(45)]
        public string Name { get; set; }
        [Required]
        [Column("secret")]
        [StringLength(32)]
        public string Secret { get; set; }
        [Column("lti_version")]
        [StringLength(12)]
        public string LtiVersion { get; set; }
        [Column("consumer_name")]
        [StringLength(255)]
        public string ConsumerName { get; set; }
        [Column("consumer_version")]
        [StringLength(255)]
        public string ConsumerVersion { get; set; }
        [Column("consumer_guid")]
        [StringLength(255)]
        public string ConsumerGuid { get; set; }
        [Column("css_path")]
        [StringLength(255)]
        public string CssPath { get; set; }
        [Column("protected")]
        public bool Protected { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }
        [Column("enable_from", TypeName = "datetime")]
        public DateTime? EnableFrom { get; set; }
        [Column("enable_until", TypeName = "datetime")]
        public DateTime? EnableUntil { get; set; }
        [Column("without_end_date")]
        public bool? WithoutEndDate { get; set; }
        [Column("last_access", TypeName = "date")]
        public DateTime? LastAccess { get; set; }
        [Column("created", TypeName = "datetime")]
        public DateTime Created { get; set; }
        [Column("updated", TypeName = "datetime")]
        public DateTime Updated { get; set; }
        [Column("role")]
        public bool? Role { get; set; }
    }
}
