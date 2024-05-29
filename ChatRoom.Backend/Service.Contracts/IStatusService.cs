using Entities.Models;
using Shared.DataTransferObjects.Status;

namespace Service.Contracts
{
    public interface IStatusService
    {
        public Task<StatusDto?> GetStatusByIdAsync(int id);
    }
}
