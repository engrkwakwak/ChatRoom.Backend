using Contracts;
using Entities.ConfigurationModels;
using Entities.Exceptions;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using Shared.DataTransferObjects.Email;

namespace SmtpClientService {
    public class SmtpClientManager(IConfiguration config, ILoggerManager logger) : ISmtpClientManager {
        private readonly EmailConfiguration _emailConfig = config.GetSection("SMTPConfiguration")?
            .Get<EmailConfiguration>() ?? throw new SmptConfigurationNotFoundException("SMTPConfiguration");
        private readonly ILoggerManager _logger = logger;

        public async Task<bool> SendEmailAsync(EmailDto request) {
            using var smtp = new SmtpClient();
            try {
                var email = ConstructEmail(request);

                await smtp.ConnectAsync(_emailConfig.Server, _emailConfig.Port, true);
                await smtp.AuthenticateAsync(_emailConfig.Username, _emailConfig.Password);
                await smtp.SendAsync(email);
                return true;
            }
            catch (Exception ex) {
                _logger.LogError($"Exception encountered in {nameof(SendEmailAsync)} with message {ex.Message} {ex.StackTrace}");
                return false;
            }
            finally {
                if (smtp.IsConnected) {
                    await smtp.DisconnectAsync(true);
                }
            }
        }

        private MimeMessage ConstructEmail(EmailDto request) {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_emailConfig.From));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = request.Body };
            return email;
        }
    }
}
