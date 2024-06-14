using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetChatsByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
				CREATE PROCEDURE [dbo].[spGetChatsByUserId]
					@userId int
				AS
				BEGIN
					SELECT 
						c.f_chat_id AS ChatId,
						c.f_chat_type_id AS ChatTypeId,
						c.f_chat_name AS ChatName,
						c.f_date_created AS DateCreated,
						c.f_status_id AS StatusId
					FROM Chats c WITH(NOLOCK)
					INNER JOIN ChatMembers cm WITH(NOLOCK)
						ON c.f_chat_id = cm.f_chat_id
					WHERE cm.f_user_id = @userId;
				END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetChatsByUserId]");
        }
    }
}
