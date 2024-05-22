﻿using AutoMapper;
using Contracts;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;

namespace Service {
    internal sealed class UserService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper) : IUserService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;

        public async Task<bool> HasDuplicateEmail(string email) {
            return await _repository.User.HasDuplicateEmail(email) > 0;
        }

        public async Task<bool> HasDuplicateUsername(string username) {
            return await _repository.User.HasDuplicateUsername(username) > 0;
        }

        public async Task<UserDto> InsertUser(SignUpDto userSignUpData) {
            User userEntity = _mapper.Map<User>(userSignUpData);
            userEntity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userSignUpData.Password);

            userEntity = await _repository.User.InsertUser(userEntity);

            UserDto userToReturn = _mapper.Map<UserDto>(userEntity);

            return userToReturn;
        }

    }
}
