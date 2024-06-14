using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetChatMemberByChatIdUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spGetChatMemberByChatIdUserId]
                    @ChatId int,
                    @UserId int
                AS
                BEGIN
                    SELECT 
	                    f_chat_id as ChatId,
	                    f_user_id as UserId,
	                    f_is_admin as IsAdmin,
	                    f_last_seen_message_id as LastSeenMessageId,
	                    f_status_id as StatusId
                    FROM ChatMembers WITH (NOLOCK)
                    WHERE f_chat_id=@ChatId AND f_user_id=@UserId
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetChatMemberByChatIdUserId]");
        }
    }
}
