using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetUsersByIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spGetUsersByIds]
                    @UserIds nvarchar(100)
                AS
                BEGIN
                    SELECT 
	                    f_user_id AS UserId,
	                    f_username AS Username,
	                    f_display_name AS DisplayName,
	                    f_email AS Email,
	                    f_password_hash AS PasswordHash,
	                    f_address AS [Address],
	                    f_birthdate AS BirthDate,
	                    f_is_email_verified AS IsEmailVerified,
	                    f_display_picture_url AS DisplayPictureUrl
                    FROM Users WITH (NOLOCK)
                    WHERE f_user_id IN (SELECT value FROM string_split(@UserIds, ','));
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetUsersByIds]");
        }
    }
}
