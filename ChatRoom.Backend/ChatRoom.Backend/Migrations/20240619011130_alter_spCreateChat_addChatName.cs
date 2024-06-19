using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class alter_spCreateChat_addChatName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[spCreateChat]
    @ChatTypeId int,
	@ChatName NVARCHAR(50)
AS
BEGIN
    INSERT INTO Chats(
	    f_chat_type_id, 
	    f_date_created, 
	    f_status_id,
		f_chat_name)
    OUTPUT 
	    inserted.f_chat_id as ChatId, 
	    inserted.f_chat_type_id as ChatTypeId,
	    inserted.f_chat_name as ChatName,
	    inserted.f_display_picture_url as DisplayPictureUrl,
	    inserted.f_date_created as DateCreated,
	    inserted.f_status_id as StatusId
    VALUES(@ChatTypeId, GETDATE(), 1, @ChatName);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spCreateChat];");
        }
    }
}
