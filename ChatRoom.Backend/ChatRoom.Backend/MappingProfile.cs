using AutoMapper;
using Entities.Models;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Chats;
using Shared.DataTransferObjects.Contacts;
using Shared.DataTransferObjects.Messages;
using Shared.DataTransferObjects.Status;
using Shared.DataTransferObjects.Users;

namespace ChatRoom.Backend {
    public class MappingProfile : Profile {
        public MappingProfile() 
        {
            CreateMap<SignUpDto, User>();
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<Contact, ContactDto>();
            CreateMap<ContactForCreationDto, Contact>();
            CreateMap<Status, StatusDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<Chat, ChatDto>().ReverseMap();
            CreateMap<MessageForCreationDto, Message > ();
            CreateMap<Message, MessageDto > ();
        }
    }
}
