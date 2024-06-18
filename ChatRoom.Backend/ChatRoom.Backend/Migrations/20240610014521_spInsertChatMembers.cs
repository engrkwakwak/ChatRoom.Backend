using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spInsertChatMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
				CREATE OR ALTER PROCEDURE [dbo].[spInsertChatMembers]
					@ChatId int,
					@UserIds tvpUserIds READONLY
				AS
				BEGIN
					SET NOCOUNT ON;

					DECLARE @InsertedRows TABLE (
						ChatId int,
						IsAdmin bit,
						StatusId int,
						UserId int
					);

					INSERT INTO ChatMembers (
						f_chat_id,
						f_is_admin,
						f_status_id,
						f_user_id
					)
					OUTPUT 
						inserted.f_chat_id AS ChatId,
						inserted.f_is_admin AS IsAdmin,
						inserted.f_status_id AS StatusId,
						inserted.f_user_id AS UserId
					INTO @InsertedRows
					SELECT 
						@ChatId, 0, 1, UserId 
					FROM @UserIds;

					SELECT 
						ir.ChatId,
						ir.IsAdmin,
						ir.StatusId,
						ir.UserId,
						u.f_display_name AS DisplayName,
						u.f_display_picture_url AS DisplayPictureUrl
					FROM @InsertedRows ir
					INNER JOIN Users u
						ON u.f_user_id = ir.UserId;
				END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("DROP PROCEDURE [dbo].[spInsertChatMembers];");
        }
    }
}
