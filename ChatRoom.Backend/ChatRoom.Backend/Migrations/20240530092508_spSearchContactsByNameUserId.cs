using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spSearchContactsByNameUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spSearchContactsByNameUserId]
                    @PageSize int,
                    @PageNumber int,
                    @UserId int,
                    @Name nvarchar(50)
                AS
                BEGIN
                    SELECT 
                        Users.f_user_id AS UserId,
                        Users.f_username AS Username,
                        Users.f_display_name AS DisplayName,
                        Users.f_email AS Email,
                        Users.f_password_hash AS PasswordHash,
                        Users.f_address AS [Address],
                        Users.f_birthdate AS BirthDate,
                        Users.f_is_email_verified AS IsEmailVerified,
	                    Users.f_display_picture_url AS DisplayPictureUrl 
                    FROM Contacts WITH (NOLOCK)
                    RIGHT JOIN Users WITH (NOLOCK)
                    ON Users.f_user_id=Contacts.f_contact_id
                    WHERE Contacts.f_user_id=1
                        AND Contacts.f_status_id = 2
                        AND (Users.f_display_name LIKE '%' + @Name + '%' 
	                        OR Users.f_display_name LIKE @Name + '%'
	                        OR Users.f_display_name LIKE '%' + @Name
	                        OR Users.f_display_name = @Name)
                    ORDER BY Contacts.f_date_created DESC
                    OFFSET (@PageNumber-1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY;
                END

            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE [dbo].[spSearchContactsByNameUserId]");
        }
    }
}
