using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Configuration {
    public class MessageTypeConfiguration : IEntityTypeConfiguration<MessageType> {
        public void Configure(EntityTypeBuilder<MessageType> builder) {
            builder.HasData(
                new MessageType {
                    MsgTypeId = 1,
                    MsgTypeName = "Normal",
                    MsgTypeDescription = "A 'Normal' message is a user-generated message, typically used for direct communication between users within a chat."
                },
                new MessageType {
                    MsgTypeId= 2,
                    MsgTypeName = "Notification",
                    MsgTypeDescription = "A 'Notification' message is a system-generated message, used to inform users about updates, events, or actions related to the chat or the application."
                });
        }
    }
}
