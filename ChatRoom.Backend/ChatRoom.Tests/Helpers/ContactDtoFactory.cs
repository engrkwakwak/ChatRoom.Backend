using Castle.Core.Resource;
using Entities.Models;
using Shared.DataTransferObjects.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatRoom.UnitTest.Helpers
{
    public class ContactDtoFactory
    {
        public static ContactForCreationDto CreateContactForCreationDto(int userId = 1, int contactId = 2, int statusId = 1)
        {
            return new()
            {
                ContactId = contactId,
                UserId = userId,
                StatusId = statusId,
            };
        }

        public static ContactDto CreateContactDto(int userId = 1, int contactId = 2, int statusId = 2)
        {
            return new()
            {
                ContactId= contactId,
                UserId = userId,
                StatusId = statusId
            };
        }

        public static Contact CreateContact(int contactId=2, int userId=1, int statusId=1)
        {
            return new()
            {
                ContactId=contactId,
                UserId=userId,
                StatusId=statusId
            };
        }
    }
}
