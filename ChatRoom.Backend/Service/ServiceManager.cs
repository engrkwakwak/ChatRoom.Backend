﻿using AutoMapper;
using Contracts;
using Microsoft.Extensions.Configuration;
using RedisCacheService;
using Service.Contracts;

namespace Service {
    public class ServiceManager(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IConfiguration configuration, IRedisCacheManager cache, IFileManager fileManager) : IServiceManager {
        private readonly Lazy<IChatMemberService> _chatMemberService = new(() => new ChatMemberService(repository, logger, mapper, cache));
        private readonly Lazy<IChatService> _chatService = new(() => new ChatService(repository, logger, mapper, cache, fileManager));
        private readonly Lazy<IContactService> _contactService = new(() => new ContactService(repository, logger, mapper, cache));
        private readonly Lazy<IMessageService> _messageService = new(() => new MessageService(repository, logger, mapper, cache));
        private readonly Lazy<IUserService> _userService = new(() => new UserService(repository, logger, mapper, cache, fileManager));
        private readonly Lazy<IAuthService> _authService = new(() => new AuthService(repository, logger, mapper, configuration, cache));
        private readonly Lazy<IEmailService> _emailService = new(() => new EmailService(repository, logger, mapper, configuration, cache));
        private readonly Lazy<IStatusService> _statusService = new(() => new StatusService(repository, logger, mapper, cache));
        private readonly Lazy<ISignalRService> _signalRService = new(() => new SignalRService(logger));

        public IChatMemberService ChatMemberService => _chatMemberService.Value;
        public IChatService ChatService => _chatService.Value;
        public IContactService ContactService => _contactService.Value;
        public IMessageService MessageService => _messageService.Value;
        public IUserService UserService => _userService.Value;
        public IEmailService EmailService => _emailService.Value;
        public IAuthService AuthService => _authService.Value;
        public IStatusService StatusService => _statusService.Value;
        public ISignalRService SignalRService => _signalRService.Value;
    }
}
