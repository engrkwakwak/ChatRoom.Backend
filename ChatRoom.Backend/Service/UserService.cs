﻿using AutoMapper;
using Contracts;
using Service.Contracts;

namespace Service {
    internal sealed class UserService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper) : IUserService
    {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;

        public async Task<bool> HasDuplicateEmail(string email)
        {
            if(await _repository.User.HasDuplicateEmail(email) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
