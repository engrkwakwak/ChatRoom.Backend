using Contracts;
using Dapper;
using Entities.Models;
using System.Data;

namespace Repository
{
    public class StatusRepository(IDbConnection connection) : IStatusRepository
    {
        private readonly IDbConnection _connection = connection;

        public async Task<Status?> GetStatusByIdAsync(int id)
        {
            DynamicParameters parameters = new();
            parameters.Add("StatusId", id);

            Status? status = await  _connection.QuerySingleOrDefaultAsync<Status>("spGetStatusById", parameters, commandType: CommandType.StoredProcedure);
            return status;
        }
    }
}
