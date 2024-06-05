using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spCreateChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spCreateChat]
                    @ChatTypeId int
                AS
                BEGIN
                    INSERT INTO Chats(
	                    f_chat_type_id, 
	                    f_date_created, 
	                    f_status_id)
                    OUTPUT 
	                    inserted.f_chat_id as ChatId, 
	                    inserted.f_chat_type_id as ChatTypeId,
	                    inserted.f_chat_name as ChatName,
	                    inserted.f_display_picture_url as DisplayPictureUrl,
	                    inserted.f_date_created as DateCreated,
	                    inserted.f_status_id as StatusId
                    VALUES(@ChatTypeId, GETDATE(), 1);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spCreateChat]");
        }
    }
}
