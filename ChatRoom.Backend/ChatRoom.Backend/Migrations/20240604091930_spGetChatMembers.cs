using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetChatMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spGetChatMembers]
                    @ChatId int
                AS
                BEGIN
                    SELECT 
	                    Users.f_user_id AS UserId,
	                    Users.f_username AS Username,
	                    Users.f_display_name AS DisplayName,
	                    Users.f_email AS Email,
	                    Users.f_password_hash AS PasswordHash,
	                    Users.f_address AS [Address],
	                    Users.f_birthdate AS BirthDate,
	                    Users.f_is_email_verified AS IsEmailVerified,
	                    Users.f_display_picture_url AS DisplayPictureUrl
                    FROM ChatMembers WITH (NOLOCK)
	                    RIGHT JOIN Users WITH (NOLOCK) ON Users.f_user_id=ChatMembers.f_user_id 
                    WHERE ChatMembers.f_chat_id=@ChatId
	                    AND ChatMembers.f_status_id != 3;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetChatMembers]");
        }
    }
}
