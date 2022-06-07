using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
    [Table("map_chat_elements")]
    [Index(nameof(ChatId), Name = "chat_id")]
    public partial class MapChatElements
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("chat_id", TypeName = "int(10) unsigned")]
        public uint ChatId { get; set; }
        [Required]
        [Column("question", TypeName = "text")]
        public string Question { get; set; }
        [Required]
        [Column("response", TypeName = "text")]
        public string Response { get; set; }
        [Required]
        [Column("function")]
        [StringLength(10)]
        public string Function { get; set; }

        [ForeignKey(nameof(ChatId))]
        [InverseProperty(nameof(MapChats.MapChatElements))]
        public virtual MapChats Chat { get; set; }
    }
}
