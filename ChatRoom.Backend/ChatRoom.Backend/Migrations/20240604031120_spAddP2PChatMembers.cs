using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spAddP2PChatMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spAddP2PChatMembers]
                    @ChatId int, 
                    @UserId1 int,
                    @UserId2 int
                AS
                BEGIN
                    INSERT INTO ChatMembers(
	                    f_chat_id,
	                    f_is_admin,
	                    f_status_id,
	                    f_user_id)
                    VALUES
	                    (@ChatId, 0, 1, @UserId1),
	                    (@ChatId, 0, 1, @UserId2);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spAddP2PChatMembers]");
        }
    }
}
