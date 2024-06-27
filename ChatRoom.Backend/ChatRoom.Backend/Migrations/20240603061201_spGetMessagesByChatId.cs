using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetMessagesByChatId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
 				CREATE PROCEDURE [dbo].[spGetMessagesByChatId]
					@chatId INT,
					@pageNumber INT,
					@pageSize INT
				AS
				BEGIN
					SELECT 
						f_message_id AS MessageId,
						f_chat_id AS ChatId,
						f_content AS Content,
						f_date_sent AS DateSent,
						f_sender_id AS UserId,
						u.f_display_name AS DisplayName,
						u.f_display_picture_url AS DisplayPictureUrl,
						m.f_msg_type_id AS MsgTypeId,
						mt.f_msg_type_name AS MsgTypeName,
						m.f_status_id AS StatusId,
						s.f_status_name AS StatusName
					FROM [Messages] m WITH(NOLOCK)
					INNER JOIN Users u WITH(NOLOCK)
						ON m.f_sender_id = u.f_user_id
					INNER JOIN MessageTypes mt WITH(NOLOCK)
						ON m.f_msg_type_id = mt.f_msg_type_id
					INNER JOIN [Status] s WITH(NOLOCK)
						ON m.f_status_id = s.f_status_id
					WHERE m.f_chat_id = @chatId
						AND m.f_status_id <> 3 
					ORDER BY m.f_message_id DESC
					OFFSET (@pageNumber - 1) * @pageSize ROWS
					FETCH NEXT @pageSize ROWS ONLY;
				END           
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetMessagesByChatId];");
        }
    }
}
