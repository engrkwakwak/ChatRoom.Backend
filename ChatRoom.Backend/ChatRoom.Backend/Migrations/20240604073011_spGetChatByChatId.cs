using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetChatByChatId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spGetChatByChatId]
                    @ChatId int
                AS
                BEGIN
                    SELECT 
                        f_chat_id as ChatId, 
                        f_chat_type_id as ChatTypeId,
                        f_chat_name as ChatName,
                        f_display_picture_url as DisplayPictureUrl,
                        f_date_created as DateCreated,
                        f_status_id as StatusId
                    FROM Chats WITH (NOLOCK)
                    WHERE f_chat_id=@ChatId;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetChatByChatId]");
        }
    }
}
