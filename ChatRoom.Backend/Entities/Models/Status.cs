using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models {
    public class Status {
        [Column("f_status_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int StatusId { get; set; }

        [Column("f_status_name", TypeName = "VARCHAR(10)")]
        public required string StatusName { get; set; }

        public ICollection<Contact>? Contacts { get; set; }
        public ICollection<ChatMember>? ChatMembers { get; set; }
        public ICollection<Chat>? Chats { get; set; }
        public ICollection<Message>? Messages { get; set; }
    }
}
