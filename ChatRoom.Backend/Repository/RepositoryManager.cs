using Contracts;
using System.Data;

namespace Repository {
    public sealed class RepositoryManager(IDbConnection connection) : IRepositoryManager {
        private readonly Lazy<IChatMemberRepository> _chatMemberRepo = new(() => new ChatMemberRepository(connection));
        private readonly Lazy<IChatRepository> _chatRepo = new(() => new ChatRepository(connection));
        private readonly Lazy<IContactRepository> _contactRepo = new(() => new ContactRepository(connection));
        private readonly Lazy<IMessageRepository> _messageRepo = new(() => new MessageRepository(connection));
        private readonly Lazy<IUserRepository> _userRepo = new(() => new UserRepository(connection));        
        private readonly Lazy<IStatusRepository> _statusRepo = new(() => new StatusRepository(connection));        

        public IChatMemberRepository ChatMember => _chatMemberRepo.Value;

        public IChatRepository Chat => _chatRepo.Value;

        public IContactRepository Contact => _contactRepo.Value;

        public IMessageRepository Message => _messageRepo.Value;

        public IUserRepository User => _userRepo.Value;

        public IStatusRepository Status => _statusRepo.Value;
    }
}
