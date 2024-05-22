using Entities.ConfigurationModels;
using Shared.DataTransferObjects.Email;
using Shared.DataTransferObjects.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Contracts
{
    public interface IEmailService
    {
        public Task<bool> SendEmail(EmailDto request);
    }
}
