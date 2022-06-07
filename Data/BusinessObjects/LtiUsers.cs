using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("lti_users")]
    public partial class LtiUsers
    {
        [Key]
        [Column("consumer_key")]
        public string ConsumerKey { get; set; }
        [Required]
        [Column("context_id")]
        [StringLength(255)]
        public string ContextId { get; set; }
        [Column("user_id")]
        [StringLength(255)]
        public string UserId { get; set; }
        [Column("lti_result_sourcedid")]
        [StringLength(255)]
        public string LtiResultSourcedid { get; set; }
        [Column("created", TypeName = "datetime")]
        public DateTime Created { get; set; }
        [Column("updated", TypeName = "datetime")]
        public DateTime Updated { get; set; }

        [ForeignKey(nameof(ConsumerKey))]
        [InverseProperty(nameof(LtiContexts.LtiUsers))]
        public virtual LtiContexts ConsumerKeyNavigation { get; set; }
    }
}
