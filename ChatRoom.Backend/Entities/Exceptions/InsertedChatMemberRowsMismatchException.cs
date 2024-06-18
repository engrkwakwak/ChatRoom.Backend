using Entities.Exceptions.Base;

namespace Entities.Exceptions {
    public class InsertedChatMemberRowsMismatchException(int insertedRows, int desiredRowsToInsert) : 
        InsertedRowsMismatchException($"There was an issue adding the chat members to the chatroom. Total inserted ids: {insertedRows}; Total expected ids to insert: {desiredRowsToInsert}.") {
    }
}
