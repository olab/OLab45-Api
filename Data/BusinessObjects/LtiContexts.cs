using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("lti_contexts")]
    public partial class LtiContexts
    {
        [Key]
        [Column("consumer_key")]
        public string ConsumerKey { get; set; }
        [Required]
        [Column("context_id")]
        [StringLength(255)]
        public string ContextId { get; set; }
        [Column("lti_context_id")]
        [StringLength(255)]
        public string LtiContextId { get; set; }
        [Column("lti_resource_id")]
        [StringLength(255)]
        public string LtiResourceId { get; set; }
        [Required]
        [Column("title")]
        [StringLength(255)]
        public string Title { get; set; }
        [Column("settings", TypeName = "text")]
        public string Settings { get; set; }
        [Column("primary_consumer_key")]
        [StringLength(255)]
        public string PrimaryConsumerKey { get; set; }
        [Column("primary_context_id")]
        [StringLength(255)]
        public string PrimaryContextId { get; set; }
        [Column("share_approved")]
        public bool? ShareApproved { get; set; }
        [Column("created", TypeName = "datetime")]
        public DateTime Created { get; set; }
        [Column("updated", TypeName = "datetime")]
        public DateTime Updated { get; set; }

        [InverseProperty("ConsumerKeyNavigation")]
        public virtual LtiUsers LtiUsers { get; set; }
    }
}
