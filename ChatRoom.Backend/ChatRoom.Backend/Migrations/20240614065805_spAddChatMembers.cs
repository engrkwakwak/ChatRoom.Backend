using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spAddChatMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spAddChatMembers]
                    @ChatId int, 
                    @UserIds tvpUserIds READONLY
                AS
                BEGIN
                    INSERT INTO ChatMembers(
	                    f_chat_id,
	                    f_is_admin,
	                    f_status_id,
	                    f_user_id)
                    SELECT 
                        @ChatId, 0, 1, UserId FROM @UserIds
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spAddChatMembers]");
        }
    }
}
