using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spUpdateUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
				CREATE PROCEDURE [dbo].[spUpdateUser]
					@userId INT,
					@userName NVARCHAR(20),
					@email VARCHAR(100),
					@birthdate DATETIME,
					@address NVARCHAR(100),
					@displayName NVARCHAR(50),
					@isEmailVerified BIT
				AS
				BEGIN
					UPDATE Users WITH(ROWLOCK)
					SET
						f_username = @userName,
						f_display_name = @displayName,
						f_email = @email,
						f_address = @address,
						f_birthdate = @birthdate,
						f_is_email_verified = @isEmailVerified
					WHERE f_user_id = @userId;
				END            
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("DROP PROCEDURE [dbo].[spUpdateUser]");
        }
    }
}
