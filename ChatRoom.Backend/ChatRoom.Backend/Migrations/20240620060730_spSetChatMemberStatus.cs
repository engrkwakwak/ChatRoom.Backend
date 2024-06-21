using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spSetChatMemberStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spSetChatMemberStatus]
                    @ChatId int,
                    @UserId int,
                    @StatusId int
                AS 
                BEGIN
                    UPDATE ChatMembers WITH (ROWLOCK)
                    SET f_status_id = @StatusId
                    WHERE f_chat_id=@ChatId
                        AND f_user_id=@UserId
                END
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spSetChatMemberStatus]");
        }
    }
}
