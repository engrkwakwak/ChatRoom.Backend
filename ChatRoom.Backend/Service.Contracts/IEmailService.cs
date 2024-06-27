
using Microsoft.AspNetCore.Http;
using Shared.DataTransferObjects.Email;
using Shared.DataTransferObjects.Users;

namespace Service.Contracts
{
    public interface IEmailService
    {
        public Task<bool> SendEmail(EmailDto request);

        public Task<bool> SendVerificationEmail(UserDto user, string verificationLink, string token);

        Task<bool> SendPasswordResetLink(UserDto user, string passwordResetLink, string token);
    }
}
