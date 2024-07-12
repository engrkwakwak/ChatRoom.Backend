using Entities.Exceptions.Base;

namespace Entities.Exceptions {
    public class StatusIdNotFoundException(int statusId)
        : NotFoundException($"The status with id: {statusId} doesn't exists in the database.") {
    }
}
