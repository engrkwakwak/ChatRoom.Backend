using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spUpdatePassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spUpdatePassword]
                    @PasswordHash VARCHAR(60),
                    @UserId int
                AS
                BEGIN
                    UPDATE Users WITH (ROWLOCK)
                    SET f_password_hash = @PasswordHash
                    WHERE f_user_id=@UserId
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spUpdatePassword]");
        }
    }
}
