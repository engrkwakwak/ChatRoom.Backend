using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AlterProcedure_SpGetChatMemberByChatIdAndUserId_IncludeUserInformationInTheFetchedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[spGetChatMemberByChatIdAndUserId]
    @chatId INT,
	@userId INT
AS
BEGIN
	SELECT 
		cm.f_chat_id AS ChatId,
		cm.f_is_admin AS IsAdmin,
		cm.f_last_seen_message_id AS LastSeenMessageId,
		cm.f_status_id AS StatusId,
		cm.f_user_id AS UserId,
		u.f_display_name AS DisplayName,
		u.f_display_picture_url AS DisplayPictureUrl
	FROM ChatMembers cm WITH(NOLOCK)
	INNER JOIN Users u WITH(NOLOCK)
		ON u.f_user_id = cm.f_user_id
	WHERE cm.f_chat_id = @chatId
		AND cm.f_user_id = @userId;
END 
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetChatMemberByChatIdAndUserId];");
        }
    }
}
