﻿using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;

namespace Service.Contracts {
    public interface IUserService {
        Task<UserDto> GetUserByIdAsync(int userId);

        Task<bool> HasDuplicateEmailAsync(string email);
        Task<bool> HasDuplicateUsernameAsync(string username);

        Task<UserDto> InsertUserAsync(SignUpDto signUp);

        Task UpdateUserAsync(int userId, UserForUpdateDto userForUpdate);
    }
}
