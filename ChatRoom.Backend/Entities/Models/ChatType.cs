using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models {
    public class ChatType {
        [Column("f_chat_type_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ChatTypeId { get; set; }

        [Column("f_chat_type_name", TypeName = "VARCHAR(15)")]
        public required string ChatTypeName { get; set; }

        [Column("f_chat_type_description", TypeName = "VARCHAR(500)")]
        public required string ChatTypeDescription { get; set; }

        public ICollection<Chat>? Chats { get; set; }
    }
}
