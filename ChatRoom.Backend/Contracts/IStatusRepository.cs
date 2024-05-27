using Entities.Models;

namespace Contracts
{
    public interface IStatusRepository
    {
        public Task<Status> GetStatusByIdAsync(int id);
    }
}
