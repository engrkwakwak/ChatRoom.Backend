using AutoMapper;
using Contracts;
using Entities.ConfigurationModels;
using Service.Contracts;
using MailKit.Net.Smtp;
using Shared.DataTransferObjects.Email;
using MimeKit.Text;
using MimeKit;

namespace Service
{
    internal sealed class EmailService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, EmailConfiguration emailConfig) : IEmailService
    {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly EmailConfiguration _emailConfig = emailConfig;
        private readonly SmtpClient smtp = new SmtpClient();

        public async Task<bool> SendEmail(EmailDto request)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_emailConfig.From));
                email.To.Add(MailboxAddress.Parse(request.To));
                email.Subject = request.Subject;
                email.Body = new TextPart(TextFormat.Html) { Text = request.Body };


                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_emailConfig.Server, _emailConfig.Port, true);
                await smtp.AuthenticateAsync(_emailConfig.Username, _emailConfig.Password);
                await smtp.SendAsync(email);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                smtp.Disconnect(true);
            }
        }

    }
}
