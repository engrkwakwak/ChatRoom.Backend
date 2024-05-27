using Contracts;
using Dapper;
using Entities.Models;
using System.Data;

namespace Repository
{
    public class StatusRepository(IDbConnection connection) : IStatusRepository
    {
        private readonly IDbConnection _connection = connection;

        public Task<Status> GetStatusByIdAsync(int id)
        {
            DynamicParameters parameters = new();
            parameters.Add("StatusId", id);

            return _connection.QueryFirstAsync<Status>("spGetStatusById", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
