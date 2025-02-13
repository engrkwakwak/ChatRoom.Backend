﻿using ChatRoom.Backend.Presentation.ActionFilters;
using Entities.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects.Contacts;
using Shared.DataTransferObjects.Users;
using Shared.RequestFeatures;

namespace ChatRoom.Backend.Presentation.Controllers
{
    [Route("api/contacts")]
    [ApiController]
    public class ContactsController(IServiceManager service) : ControllerBase
    {
        private readonly IServiceManager _service = service;

        [HttpPost("")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ContactForCreationDto contact)
        {
            if (contact.UserId == contact.ContactId)
            {
                throw new InvalidParameterException("Something went wrong. The request parameters are invalid");
            }

            if (await _service.UserService.GetUserByIdAsync((int)contact.UserId!) == null 
                || await _service.UserService.GetUserByIdAsync((int)contact.ContactId!) == null
                || await _service.StatusService.GetStatusByIdAsync((int)contact.StatusId!) == null)
            {
                throw new InvalidParameterException("Something went wrong. Record doesnt exist");
            }

            if (!await _service.ContactService.InsertOrUpdateContactAsync(contact))
            {
                throw new Exception("Something went wrong while processing the request.");
            }

            return new CreatedResult();
        }

        [HttpDelete("{userId}/{contactId}")]
        [Authorize]
        public async Task<IActionResult> Delete(int userId, int contactId)
        {
            if(userId < 1 || contactId < 1)
            {
                throw new InvalidParameterException("The request parameters are invalid");
            }

            if(!await _service.ContactService.DeleteContactByUserIdContactIdAsync(userId, contactId))
            {
                throw new Exception("Something went wrong while deleting the contact.");
            }
            return new OkResult();
        }

        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> ViewContacts([FromQuery] ContactParameters contactParameters)
        {
            if(contactParameters.UserId < 1)
            {
                throw new InvalidParameterException("The request parameters are invalid");
            }
            IEnumerable<UserDto> userDtos = await  _service.ContactService.SearchContactsByNameUserIdAsync(contactParameters);
            return Ok(userDtos);
        }

        [HttpGet("active/{userId}/{contactId}")]
        [Authorize]
        public async Task<IActionResult> GetActiveContactInfo(int userId, int contactId)
        {
            ContactDto? contact = await _service.ContactService.GetContactByUserIdContactIdAsync(userId, contactId);
            if(contact == null || contact.StatusId != 2)
            {
                return Ok(null);
            }
            return Ok(contact);
        }
    }
}
