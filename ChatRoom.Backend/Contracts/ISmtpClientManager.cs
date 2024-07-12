using MimeKit;
using Shared.DataTransferObjects.Email;

namespace Contracts {
    public interface ISmtpClientManager {
        Task<bool> SendEmailAsync(EmailDto request);
    }
}
