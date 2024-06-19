using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetUserCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[spGetUserCount]
    @Name nvarchar(50)
AS
BEGIN
    SELECT COUNT(1)
    FROM Users WITH (NOLOCK) 
    WHERE f_is_email_verified = 1
		AND (@Name IS NULL OR LOWER(f_display_name) LIKE '%' + LOWER(@Name) + '%');
END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetUserCount];");
        }
    }
}
