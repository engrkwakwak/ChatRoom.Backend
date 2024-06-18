using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetChatListByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spGetChatListByUserId]
                    @UserId int,
                    @PageNumber int,
                    @PageSize int
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
                    LEFT JOIN ChatMembers WITH (NOLOCK)
	                    ON Chats.f_chat_id = ChatMembers.f_chat_id
                    WHERE ChatMembers.f_user_id=@UserId
	                    AND Chats.f_status_id <> 3
	                    AND ChatMembers.f_status_id IN (1,2)
                    ORDER BY LastDateSent DESC
                    OFFSET (@PageNumber-1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetChatListByUserId]");
        }
    }
}
