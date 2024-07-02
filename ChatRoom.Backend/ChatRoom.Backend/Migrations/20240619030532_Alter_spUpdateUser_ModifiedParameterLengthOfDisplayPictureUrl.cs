using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class Alter_spUpdateUser_ModifiedParameterLengthOfDisplayPictureUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "f_display_picture_url",
                table: "Users",
                type: "VARCHAR(200)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(100)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "f_display_picture_url",
                table: "Chats",
                type: "VARCHAR(200)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(100)",
                oldNullable: true);

            migrationBuilder.Sql(@"
CREATE OR ALTER   PROCEDURE [dbo].[spUpdateUser]
	@userId INT,
	@userName NVARCHAR(20),
	@email VARCHAR(100),
	@birthdate DATETIME,
	@address NVARCHAR(100),
	@displayName NVARCHAR(50),
	@isEmailVerified BIT,
	@displayPictureUrl VARCHAR(200)
AS
BEGIN
	UPDATE Users WITH(ROWLOCK)
	SET
		f_username = @userName,
		f_display_name = @displayName,
		f_email = @email,
		f_address = @address,
		f_birthdate = @birthdate,
		f_is_email_verified = @isEmailVerified,
		f_display_picture_url = @displayPictureUrl
	WHERE f_user_id = @userId;
END 
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "f_display_picture_url",
                table: "Users",
                type: "VARCHAR(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(200)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "f_display_picture_url",
                table: "Chats",
                type: "VARCHAR(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(200)",
                oldNullable: true);

            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spUpdateUser];");
        }
    }
}
