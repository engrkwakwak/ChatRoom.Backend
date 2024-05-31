namespace Contracts {
    public interface IRepositoryManager {
        IChatMemberRepository ChatMember { get; }
        IChatRepository Chat { get; }
        IContactRepository Contact { get; }
        IMessageRepository Message { get; }
        IUserRepository User { get; }
        IStatusRepository Status { get; }
    }
}