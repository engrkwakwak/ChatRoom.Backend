using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ChatTypes",
                columns: new[] { "f_chat_type_id", "f_chat_type_description", "f_chat_type_name" },
                values: new object[,]
                {
                    { 1, "Peer-to-Peer (P2P) chat type is designed for direct, one-on-one communication between two users. This is similar to a private conversation where messages are only visible to the participating users.", "P2P" },
                    { 2, "Group Chat type is designed for multi-user conversations where messages can be seen by all members of the group. This is ideal for team discussions, project collaborations, or any scenario where multiple users need to communicate simultaneously.", "GroupChat" }
                });

            migrationBuilder.InsertData(
                table: "MessageTypes",
                columns: new[] { "f_msg_type_id", "f_msg_type_description", "f_msg_type_name" },
                values: new object[,]
                {
                    { 1, "A 'Normal' message is a user-generated message, typically used for direct communication between users within a chat.", "Normal" },
                    { 2, "A 'Notification' message is a system-generated message, used to inform users about updates, events, or actions related to the chat or the application.", "Notification" }
                });

            migrationBuilder.InsertData(
                table: "Status",
                columns: new[] { "f_status_id", "f_status_name" },
                values: new object[,]
                {
                    { 1, "Active" },
                    { 2, "Approved" },
                    { 3, "Deleted" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ChatTypes",
                keyColumn: "f_chat_type_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ChatTypes",
                keyColumn: "f_chat_type_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MessageTypes",
                keyColumn: "f_msg_type_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MessageTypes",
                keyColumn: "f_msg_type_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Status",
                keyColumn: "f_status_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Status",
                keyColumn: "f_status_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Status",
                keyColumn: "f_status_id",
                keyValue: 3);
        }
    }
}
