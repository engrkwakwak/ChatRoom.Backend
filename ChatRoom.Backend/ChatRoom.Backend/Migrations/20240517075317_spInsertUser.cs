using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spInsertUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            SET ANSI_NULLS ON
            GO
            SET QUOTED_IDENTIFIER ON
            GO
            CREATE PROCEDURE spInsertUser
	            @Username nvarchar(20),
	            @DisplayName nvarchar(50),
	            @Email varchar(100),
	            @PasswordHash varchar(60)
            AS
            BEGIN
	            SET NOCOUNT ON;
	            INSERT INTO Users(f_username, f_display_name, f_email, f_password_hash, f_is_email_verified, f_date_created) 
                OUTPUT inserted.f_user_id as UserId, inserted.f_username as Username, inserted.f_display_name as DisplayName, inserted.f_email as Email, inserted.f_is_email_verified as IsEmailVerifiedAt, inserted.f_date_created as DateCreated
	            VALUES(@Username, @DisplayName, @Email, @PasswordHash, 0, GETDATE());
            END
            GO
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE spInsertUser");
        }
    }
}
