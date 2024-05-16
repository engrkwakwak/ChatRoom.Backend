using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models {
    public class MessageType {
        [Column("f_msg_type_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int MsgTypeId { get; set; }

        [Column("f_msg_type_name", TypeName = "VARCHAR(15)")]
        public required string MsgTypeName { get; set; }

        [Column("f_msg_type_description", TypeName = "VARCHAR(500)")]
        public required string MsgTypeDescription { get; set; }

        public ICollection<Message>? Messages { get; set; }
    }
}
