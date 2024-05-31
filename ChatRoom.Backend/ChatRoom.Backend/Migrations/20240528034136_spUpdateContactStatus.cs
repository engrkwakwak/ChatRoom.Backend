using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spUpdateContactStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spUpdateContactStatus]
                    @user_id int,
                    @contact_id int,
                    @status_id int
                AS
                BEGIN
                    UPDATE Contacts WITH (ROWLOCK)
                    SET f_status_id=@status_id    
                    WHERE f_user_id=@user_id
	                    AND f_contact_id=@contact_id;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spUpdateContactStatus];");
        }
    }
}
