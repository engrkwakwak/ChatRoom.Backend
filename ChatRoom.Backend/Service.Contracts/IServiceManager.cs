namespace Service.Contracts {
    public interface IServiceManager {
        IChatMemberService ChatMemberService { get; }
        IChatService ChatService { get; }
        IContactService ContactService { get; }
        IMessageService MessageService { get; }
        IUserService UserService { get; }
        IEmailService EmailService { get; }
        IAuthService AuthService { get; }
        IFileService FileService { get; }
        IStatusService StatusService { get; }
    }
}
