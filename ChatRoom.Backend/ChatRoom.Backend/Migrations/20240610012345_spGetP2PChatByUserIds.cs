using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetP2PChatByUserIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spGetPToPChatByUserIds]
                    @UserId1 int,
                    @UserId2 int
                AS
                BEGIN
					SELECT 
						c.f_chat_id AS ChatId,
						c.f_chat_type_id AS ChatTypeId,
						c.f_chat_name AS ChatName,
						c.f_date_created AS DateCreated,
						c.f_status_id AS StatusId
					FROM Chats c WITH(NOLOCK)
					INNER JOIN ChatMembers cm1 WITH(NOLOCK)
						ON c.f_chat_id = cm1.f_chat_id
					INNER JOIN ChatMembers cm2 WITH(NOLOCK)
						ON c.f_chat_id = cm2.f_chat_id
					WHERE cm1.f_user_id = @UserId1
						AND cm2.f_user_id = @UserId2
						AND c.f_status_id = 1
						AND c.f_chat_type_id = 1; -- P2P
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetPToPChatByUserIds]");
        }
    }
}
