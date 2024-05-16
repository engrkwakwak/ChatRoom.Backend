using AutoMapper;
using Contracts;
using Service.Contracts;

namespace Service {
    public class ServiceManager(IRepositoryManager repository, ILoggerManager logger, IMapper mapper) : IServiceManager {
        private readonly Lazy<IChatMemberService> _chatMemberService = new(() => new ChatMemberService(repository, logger, mapper));
        private readonly Lazy<IChatService> _chatService = new(() => new ChatService(repository, logger, mapper));
        private readonly Lazy<IContactService> _contactService = new(() => new ContactService(repository, logger, mapper));
        private readonly Lazy<IMessageService> _messageService = new(() => new MessageService(repository, logger, mapper));
        private readonly Lazy<IUserService> _userService = new(() => new UserService(repository, logger, mapper));

        public IChatMemberService ChatMemberService => _chatMemberService.Value;
        public IChatService ChatService => _chatService.Value;
        public IContactService ContactService => _contactService.Value;
        public IMessageService MessageService => _messageService.Value;
        public IUserService UserService => _userService.Value;
    }
}
