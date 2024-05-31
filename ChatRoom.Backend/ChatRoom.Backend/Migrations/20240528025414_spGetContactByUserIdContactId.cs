using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetContactByUserIdContactId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spGetContactByUserIdContactId]
                    @user_id int,
                    @contact_id int
                AS
                BEGIN
                    SELECT 
	                    f_user_id AS UserId,
	                    f_contact_id AS ContactId,
	                    f_status_id AS StatusId,
	                    f_date_created AS DateCreated,
	                    f_date_updated AS DateUpdated
                    FROM Contacts WITH (NOLOCK)
                    WHERE f_user_id = @user_id	
                        AND f_contact_id = @contact_id;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetContactByUserIdContactId]");
        }
    }
}
