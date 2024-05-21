using AutoMapper;
using Contracts;
using Entities.ConfigurationModels;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    internal sealed class EmailService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, EmailConfiguration emailConfig) : IEmailService
    {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly EmailConfiguration _emailConfig = emailConfig;

        public bool SendEmailVerification()
        {
            throw new NotImplementedException();
        }
    }
}
