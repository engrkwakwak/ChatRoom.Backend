using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetChatMemberByChatIdAndUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[spGetChatMemberByChatIdAndUserId]
    @chatId INT,
	@userId INT
AS
BEGIN
    SELECT 
		cm.f_chat_id AS ChatId,
		cm.f_is_admin AS IsAdmin,
		cm.f_last_seen_message_id AS LastSeenMessageId,
		cm.f_status_id AS StatusId,
		cm.f_user_id AS UserId
    FROM ChatMembers cm WITH (NOLOCK)
    WHERE cm.f_chat_id = @chatId
	    AND cm.f_user_id = @userId;
END           
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetChatMemberByChatIdAndUserId]");
        }
    }
}
