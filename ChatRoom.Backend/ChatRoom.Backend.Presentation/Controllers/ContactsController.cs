using ChatRoom.Backend.Presentation.ActionFilters;
using Entities.Exceptions;
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

        [HttpPost("")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> Create([FromBody] ContactForCreationDto contact)
        {
            if (contact.UserId == contact.ContactId)
            {
                throw new InvalidParameterException("Something went wrong. The request parameters are invalid");
            }

            if (await _service.UserService.GetUserById((int)contact.UserId!) == null 
                || await _service.UserService.GetUserById((int)contact.ContactId!) == null
                || await _service.StatusService.GetStatusById((int)contact.StatusId!) == null)
            {
                throw new InvalidParameterException("Something went wrong. Record doesnt exist");
            }

            if (!await _service.ContactService.InsertOrUpdateContact(contact))
            {
                throw new Exception("Something went wrong while processing the request.");
            }

            return new CreatedResult();
        }

        [HttpDelete("{userId}/{contactId}")]
        public async Task<IActionResult> Delete(int userId, int contactId)
        {
            if(userId < 1 || contactId < 1)
            {
                throw new InvalidParameterException("The request parameters are invalid");
            }

            if(!await _service.ContactService.DeleteContactByUserIdContactId(userId, contactId))
            {
                throw new Exception("Something went wrong while deleting the contact.");
            }
            return new OkResult();
        }

        // view contacts
        [HttpGet("")]
        public async Task<IActionResult> ViewContacts([FromQuery] ContactParameters contactParameters)
        {
            if(contactParameters.UserId < 1)
            {
                throw new InvalidParameterException("The request parameters are invalid");
            }
            IEnumerable<ContactDto> contactDtos = await  _service.ContactService.GetContactsByUserId(contactParameters);
            return Ok(contactDtos);
        }

    }
}
