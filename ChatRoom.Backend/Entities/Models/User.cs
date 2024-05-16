using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models {
    public class User {
        [Column("f_user_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int UserId { get; set; }

        [Column("f_username", TypeName = "NVARCHAR(20)")]
        public required string Username { get; set; }

        [Column("f_display_name", TypeName = "NVARCHAR(50)")]
        public required string DisplayName { get; set; }

        [Column("f_email", TypeName = "VARCHAR(100)")]
        public required string Email { get; set; }

        [Column("f_password_hash", TypeName = "VARCHAR(60)")]
        public required string PasswordHash { get; set; }

        [Column("f_address", TypeName = "NVARCHAR(100)")]
        public string? Address { get; set; }

        [Column("f_birthdate")]
        public DateTime? BirthDate { get; set; }

        [Column("f_is_email_verified")]
        public bool IsEmailVerified { get; set; }

        [Column("f_display_picture_url", TypeName = "VARCHAR(100)")]
        public string? DisplayPictureUrl { get; set; }

        [Column("f_date_created")]
        public DateTime? DateCreated { get; set; }

        public ICollection<Contact>? Contacts { get; set; }
        public ICollection<ChatMember>? ParticipatedChats { get; set; }
        public ICollection<Message>? Messages { get; set; }
    }
}
