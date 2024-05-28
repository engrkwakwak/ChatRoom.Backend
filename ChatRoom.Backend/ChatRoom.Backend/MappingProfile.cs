using AutoMapper;
using Entities.Models;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Contacts;
using Shared.DataTransferObjects.Users;

namespace ChatRoom.Backend {
    public class MappingProfile : Profile {
        public MappingProfile() 
        {
            CreateMap<SignUpDto, User>();
            CreateMap<User, UserDto>();
            CreateMap<Contact, ContactDto>();
        }
    }
}
