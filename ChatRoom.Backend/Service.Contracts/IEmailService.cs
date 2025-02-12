﻿
using Microsoft.AspNetCore.Http;
using Shared.DataTransferObjects.Email;
using Shared.DataTransferObjects.Users;

namespace Service.Contracts
{
    public interface IEmailService
    {
        public Task<bool> SendVerificationEmail(UserDto user, string verificationLink, string token);

        Task<bool> SendPasswordResetLink(UserDto user, string passwordResetLink, string token);

        Task RemoveTokenFromCache(string token);

        Task<bool> IsEmailTokenUsed(string token);
    }
}
