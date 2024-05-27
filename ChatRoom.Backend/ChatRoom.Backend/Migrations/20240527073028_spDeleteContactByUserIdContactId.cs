using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spDeleteContactByUserIdContactId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spDeleteContactByUserIdContactId]
                    @UserId int,
                    @ContactId int
                AS
                BEGIN
                    UPDATE Contacts WITH (ROWLOCK)
                    SET f_status_id=3
                    WHERE f_user_id=@UserId
                    AND f_contact_id=@ContactId;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spDeleteContactByUserIdContactId]");
        }
    }
}
