using ChatRoom.Backend.Presentation.ActionFilters;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects.Contacts;
using Shared.RequestFeatures;

namespace ChatRoom.Backend.Presentation.Controllers
{
    [Route("api/contacts")]
    [ApiController]
    public class ContactsController(IServiceManager service) : ControllerBase
    {
        private IServiceManager _service = service;
        // add contact
        [HttpPost("")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> Create([FromBody] ContactForCreationDto contact)
        {
            if (!ModelState.IsValid)
            {
                // temp
                return BadRequest("Error");
            }

            if(contact.UserId == contact.ContactId)
            {
                return BadRequest("Invalid Request ");
            }

            // check if userid, contactid and status id exist in the database
            // if exist update
            // if not create
            //if(_service.)
            ContactDto contactDto = await _service.ContactService.GetContactByUserIdContactId((int)contact.UserId!, (int)contact.ContactId!);
            return Ok(contactDto);
        }

        // delete contact
        [HttpDelete("{userId}/{contactId}")]
        public IActionResult Delete(int userId, int contactId)
        {
            return Ok("Contacts Deleted");
        }

        // view contacts
        [HttpGet("")]
        public IActionResult ViewContacts([FromRoute] ContactParameters contactParameters)
        {
            return Ok("Contacts Fetched");
        }

    }
}
