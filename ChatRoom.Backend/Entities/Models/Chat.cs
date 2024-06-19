using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models {
    public class Chat {
        [Column("f_chat_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ChatId { get; set; }

        [Column("f_chat_type_id")]
        [ForeignKey(nameof(ChatType))]
        public int ChatTypeId { get; set; }
        public ChatType? ChatType { get; set; }

        [Column("f_chat_name", TypeName = "NVARCHAR(50)")]
        public string? ChatName { get; set; }

        [Column("f_display_picture_url", TypeName = "VARCHAR(200)")]
        public string? DisplayPictureUrl { get; set; }

        [Column("f_date_created")]
        public DateTime? DateCreated { get; set; }

        [Column("f_status_id")]
        [ForeignKey(nameof(Status))]
        public int StatusId { get; set; }
        public Status? Status { get; set; }

        public IEnumerable<ChatMember>? Members { get; set; }
        public IEnumerable<Message>? Messages { get; set; }
    }
}
