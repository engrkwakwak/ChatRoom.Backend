using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spDeleteMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spDeleteMessage]
                    @MessageId int
                AS
                BEGIN
                    UPDATE [Messages] WITH (ROWLOCK)
                    SET f_status_id=3
                    WHERE f_message_id=@MessageId
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spDeleteMessage]");
        }
    }
}
