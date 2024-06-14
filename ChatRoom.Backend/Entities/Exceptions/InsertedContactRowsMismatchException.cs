using Entities.Exceptions.Base;

namespace Entities.Exceptions {
    public class InsertedContactRowsMismatchException(int insertedRows, int desiredRowsToInsert) :
        InsertedRowsMismatchException($"There was an issue adding the contacts to the chatroom. Total inserted ids: {insertedRows}; Total expected ids to insert: {desiredRowsToInsert}.") {

    }
}
