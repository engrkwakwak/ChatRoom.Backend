using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spSearchChatlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spSearchChatlist]
                    @UserId int,
                    @PageNumber int,
                    @PageSize int,
                    @Name nvarchar(100)
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT
						Chats.f_chat_id as ChatId,
						Chats.f_chat_name as ChatName,
						Chats.f_chat_type_id as ChatTypeId,
						Chats.f_date_created as DateCreated,
						Chats.f_display_picture_url as DisplayPictureUrl,
						Chats.f_status_id,
						(
							SELECT TOP 1 [Messages].f_date_sent 
							FROM [Messages] WITH (NOLOCK)
							WHERE [Messages].f_chat_id=Chats.f_chat_id
								AND [Messages].f_status_id = 1
							ORDER BY [Messages].f_date_sent DESC
						)  AS LastDateSent
					FROM Chats WITH (NOLOCK)
					WHERE 
						(
							SELECT COUNT(1)
							FROM [Messages] WITH (NOLOCK)
							WHERE [Messages].f_chat_id=Chats.f_chat_id
								AND [Messages].f_status_id = 1
						) > 0
						AND 
						Chats.f_status_id <> 3
						AND 
						(
							EXISTS (
								SELECT * FROM ChatMembers 
								LEFT JOIN Users ON Users.f_user_id=ChatMembers.f_user_id
								WHERE Users.f_display_name LIKE '%' + @Name + '%'
								AND Chats.f_chat_id=ChatMembers.f_chat_id
							)
							OR Chats.f_chat_name LIKE '%' + @Name + '%' 
						)
						AND EXISTS (
							SELECT * FROM ChatMembers 
							WHERE ChatMembers.f_user_id = @UserId
							AND Chats.f_chat_id=ChatMembers.f_chat_id 
							AND ChatMembers.f_status_id IN (1,2)
						)
					ORDER BY LastDateSent DESC
					OFFSET (@PageNumber-1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("DROP PROCEDURE [dbo].[spSearchChatlist]");
        }
    }
}
