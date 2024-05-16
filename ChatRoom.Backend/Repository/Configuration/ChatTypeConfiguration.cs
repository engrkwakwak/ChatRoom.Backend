using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Configuration {
    public class ChatTypeConfiguration : IEntityTypeConfiguration<ChatType> {
        public void Configure(EntityTypeBuilder<ChatType> builder) {
            builder.HasData(
                new ChatType {
                    ChatTypeId = 1,
                    ChatTypeName = "P2P",
                    ChatTypeDescription = "Peer-to-Peer (P2P) chat type is designed for direct, one-on-one communication between two users. This is similar to a private conversation where messages are only visible to the participating users."
                },
                new ChatType {
                    ChatTypeId = 2,
                    ChatTypeName = "GroupChat",
                    ChatTypeDescription = "Group Chat type is designed for multi-user conversations where messages can be seen by all members of the group. This is ideal for team discussions, project collaborations, or any scenario where multiple users need to communicate simultaneously."
                });
        }
    }
}