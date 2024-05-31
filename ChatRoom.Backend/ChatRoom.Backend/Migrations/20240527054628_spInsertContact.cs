using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spInsertContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE PROCEDURE [dbo].[spInsertContact]
                @UserId int,
                @ContactId int,
                @StatusId int
            AS
            BEGIN
                SET NOCOUNT ON;
                INSERT INTO Contacts(
	                f_user_id, 
	                f_contact_id, 
	                f_status_id, 
	                f_date_created, 
	                f_date_updated) 
                OUTPUT inserted.f_user_id AS UserId, 
	                inserted.f_contact_id AS ContactId, 
	                inserted.f_status_id AS StatusId, 
	                inserted.f_date_created AS DateCreated, 
	                inserted.f_date_updated AS DateUpdated
                VALUES(@UserId, @ContactId, @StatusId, GETDATE(), GETDATE());
            END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spInsertContact]");
        }
    }
}
