using AutoMapper;
using Contracts;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.Status;

namespace Service
{
    internal sealed class StatusService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper) : IStatusService
    {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;

        public async Task<StatusDto?> GetStatusByIdAsync(int id)
        {
            Status? status = await _repository.Status.GetStatusByIdAsync(id);
            return status == null ? null : _mapper.Map<StatusDto?>(status);
        }
    }
}
