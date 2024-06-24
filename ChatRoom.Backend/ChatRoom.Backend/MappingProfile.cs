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
            CreateMap<Chat, ChatDto>().ReverseMap();
            CreateMap<ChatForCreationDto, Chat>();
            CreateMap<ChatForUpdateDto, Chat>();
            CreateMap<ChatMember, ChatMemberDto>().ReverseMap();
            CreateMap<ChatMember, ChatMemberForUpdateDto>().ReverseMap();

            CreateMap<Contact, ContactDto>();
            CreateMap<ContactForCreationDto, Contact>();

            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.Sender, 
                    opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.MessageType,
                    opt => opt.MapFrom(src => src.MessageType))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status));
            CreateMap<MessageType, MessageTypeDto>();
            CreateMap<MessageForCreationDto, Message>();

            CreateMap<Status, StatusDto>();

            CreateMap<SignUpDto, User>();
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<User, UserDisplayDto>().ReverseMap();
        }
    }
}
