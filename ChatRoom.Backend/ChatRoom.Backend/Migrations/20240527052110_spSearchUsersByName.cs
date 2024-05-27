using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spSearchUsersByName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spSearchUsersByName]
                    @PageSize int,
                    @PageNumber int,                    
                    @Name nvarchar(50)
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
                    WHERE f_is_email_verified = 1
                    AND (f_display_name LIKE '%' + @Name + '%' 
	                    OR f_display_name LIKE @Name + '%'
	                    OR f_display_name LIKE '%' + @Name
	                    OR f_display_name = @Name)
                    ORDER BY f_display_name DESC
                    OFFSET (@PageNumber-1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spSearchUsersByName]");
        }
    }
}
