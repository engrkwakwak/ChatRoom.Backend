using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetMessageCountByChatId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
				CREATE PROCEDURE [dbo].[spGetMessageCountByChatId]
					@chatId INT
				AS
				BEGIN
					SELECT COUNT(1)
					FROM [Messages] WITH(NOLOCK)
					WHERE f_chat_id = @chatId
                        AND f_status_id <> 3;
				END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetMessageCountByChatId];");
        }
    }
}
