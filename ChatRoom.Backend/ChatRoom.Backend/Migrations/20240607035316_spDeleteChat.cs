using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spDeleteChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spDeleteChat]
                    @ChatId int
                AS
                BEGIN   
                    UPDATE [Chats] WITH (ROWLOCK)
                    SET f_status_id = 3
                    WHERE f_chat_id=@ChatId
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spDeleteChat]");
        }
    }
}
