
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.Contacts
{
    public class ContactForCreationDto
    {
        [DisplayName("User")]
        [Required]
        public int? UserId { get; set; }

        [DisplayName("Contact")]
        [Required]
        public int? ContactId { get; set; }

        [DisplayName("Status")]
        [Required]
        public int? StatusId { get; set; }
    }
}
