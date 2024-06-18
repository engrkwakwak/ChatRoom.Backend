using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spUpdateMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spUpdateMessage]
                    @MessageId int,
                    @Content nvarchar(MAX)
                AS
                BEGIN
                    UPDATE [Messages] WITH (ROWLOCK)
                    SET f_content=@Content, f_date_updated=GETDATE()
                    WHERE f_message_id=@MessageId
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spUpdateMessage]");
        }
    }
}
