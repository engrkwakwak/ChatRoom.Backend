using AutoMapper;
using Contracts;
using RedisCacheService;
using Service.Contracts;

namespace Service {
    internal sealed class ChatMemberService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IRedisCacheManager cache) : IChatMemberService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;
    }
}
