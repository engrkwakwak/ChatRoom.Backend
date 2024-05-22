using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spHasDuplicateUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            SET ANSI_NULLS ON
            GO
            SET QUOTED_IDENTIFIER ON
            GO
            CREATE PROCEDURE spHasDuplicateUsername
	            @Username nvarchar(20)
            AS
            BEGIN
	            SET NOCOUNT ON;
	            SELECT COUNT(1) FROM Users WITH(NOLOCK) WHERE f_username=@Username;
            END
            GO
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE spHasDuplicateUsername");
        }
    }
}
