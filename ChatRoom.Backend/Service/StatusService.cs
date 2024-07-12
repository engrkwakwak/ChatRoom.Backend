using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using RedisCacheService;
using Service.Contracts;
using Shared.DataTransferObjects.Status;

namespace Service;

public sealed class StatusService(
    IRepositoryManager repository,
    ILoggerManager logger,
    IMapper mapper,
    IRedisCacheManager cache
) : IStatusService {

    private readonly IRepositoryManager _repository = repository;
    private readonly ILoggerManager _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IRedisCacheManager _cache = cache;

    public async Task<StatusDto> GetStatusByIdAsync(int id)
    {
        string cacheKey = $"status:{id}";
        Status status = await _cache.GetCachedDataAsync<Status>(cacheKey);
        if (status != null)
        {
            return _mapper.Map<StatusDto>(status);
        }
        status = await _repository.Status.GetStatusByIdAsync(id) 
            ?? throw new StatusIdNotFoundException(id);
        return _mapper.Map<StatusDto>(status);
    }
}
