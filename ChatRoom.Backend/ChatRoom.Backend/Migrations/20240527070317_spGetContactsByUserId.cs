using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetContactsByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spGetContactsByUserId]
                    @PageSize int,
                    @PageNumber int,
                    @UserId int
                AS
                BEGIN
                    SELECT 
	                    Contacts.f_user_id AS UserId,
	                    Contacts.f_contact_id AS ContactId,
	                    Contacts.f_status_id AS StatusId,
	                    Contacts.f_date_created AS DateCreated,
	                    Contacts.f_date_updated AS DateUpdated
                    FROM Contacts WITH (NOLOCK)
                    WHERE Contacts.f_user_id=@UserId
	                    AND Contacts.f_status_id = 2
                    ORDER BY Contacts.f_date_created DESC
                    OFFSET (@PageNumber-1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetContactsByUserId]");
        }
    }
}
