using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetPToPChatIdByUserIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spGetPToPChatIdByUserIds]
                    @UserId1 int,
                    @UserId2 int
                AS
                BEGIN
                    SELECT ChatMembers.f_chat_id FROM ChatMembers WITH (NOLOCK)
                    JOIN Chats ON ChatMembers.f_chat_id=Chats.f_chat_id
                    WHERE ChatMembers.f_user_id IN (@UserId1,@UserId2)
                    AND Chats.f_status_id=1
                    GROUP BY ChatMembers.f_chat_id 
                    HAVING COUNT(1) > 1;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetPToPChatIdByUserIds]");
        }
    }
}
