using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class CreateSp_SpUpdateChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[spUpdateChat]
	@ChatId INT,
	@ChatName NVARCHAR(50),
	@DisplayPictureUrl VARCHAR(200)
AS
BEGIN
	UPDATE Chats WITH(ROWLOCK)
	SET
		f_chat_name = @ChatName,
		f_display_picture_url = @DisplayPictureUrl
	WHERE f_chat_id = @ChatId;
END 
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spUpdateChat];");
        }
    }
}
