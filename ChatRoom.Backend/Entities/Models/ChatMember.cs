using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models {
    public class ChatMember {
        [Column("f_chat_id")]
        [ForeignKey(nameof(Chat))]
        public int ChatId { get; set; }
        public Chat? Chat { get; set; }

        [Column("f_user_id")]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Column("f_is_admin")]
        public bool IsAdmin { get; set; }

        [Column("f_last_seen_message_id")]
        [ForeignKey(nameof(Message))]
        public int LastSeenMessageId { get; set; }
        public Message? Message { get; set; }

        [Column("f_status_id")]
        [ForeignKey(nameof(Status))]
        public int StatusId { get; set; }
        public Status? Status { get; set; }
    }
}
