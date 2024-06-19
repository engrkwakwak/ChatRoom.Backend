using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[spGetUsers]
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
        f_address AS [Address],
        f_birthdate AS BirthDate,
		f_display_picture_url AS DisplayPictureUrl 
    FROM Users WITH (NOLOCK) 
    WHERE f_is_email_verified = 1
		AND (@Name IS NULL OR LOWER(f_display_name) LIKE '%' + LOWER(@Name) + '%')
    ORDER BY f_display_name DESC
    OFFSET (@PageNumber-1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetUsers];");
        }
    }
}
