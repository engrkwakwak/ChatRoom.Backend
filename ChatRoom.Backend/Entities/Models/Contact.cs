using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models {
    public class Contact {
        [Column("f_user_id")]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Column("f_contact_id")]
        [ForeignKey(nameof(UserContact))]
        public int ContactId { get; set; }
        public User? UserContact { get; set; }

        [Column("f_status_id")]
        [ForeignKey(nameof(Status))]
        public int StatusId { get; set; }
        public Status? Status { get; set; }

        [Column("f_date_created")]
        public DateTime? DateCreated { get; set; }

        [Column("f_date_updated")]
        public DateTime? DateUpdated { get; set;}
    }
}
