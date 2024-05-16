using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models {
    public class Message {
        [Column("f_message_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int MessageId { get; set; }

        [Column("f_chat_id")]
        [ForeignKey(nameof(Chat))]
        public int ChatId { get; set; }
        public Chat? Chat { get; set; }

        [Column("f_sender_id")]
        [ForeignKey(nameof(User))]
        public int SenderId { get; set; }
        public User? User { get; set; }

        [Column("f_msg_type_id")]
        [ForeignKey(nameof(MessageType))]
        public int MsgTypeId { get; set; }
        public MessageType? MessageType { get; set; }

        [Column("f_content", TypeName = "NVARCHAR(MAX)")]
        public required string Content { get; set; }

        [Column("f_date_sent")]
        public DateTime DateSent { get; set; }

        [Column("f_date_updated")]
        public DateTime DateUpdated { get; set; }

        [Column("f_status_id")]
        [ForeignKey(nameof(Status))]
        public int StatusId { get; set; }
        public Status? Status { get; set; }

        public ICollection<ChatMember>? LastSeenUsers { get; set; }
    }
}