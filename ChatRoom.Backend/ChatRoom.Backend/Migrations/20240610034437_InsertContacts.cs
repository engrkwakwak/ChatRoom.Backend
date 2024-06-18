using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class InsertContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
				CREATE PROCEDURE [dbo].[spInsertContacts]
					@userId int,
					@contactIds tvpUserIds READONLY
				AS
				BEGIN
					MERGE INTO Contacts AS Target
					USING (
						SELECT DISTINCT @userId AS UserId, c.UserId AS ContactId
						FROM @contactIds c
					) AS Source
					ON Target.f_user_id = Source.UserId AND Target.f_contact_id = Source.ContactId
					WHEN MATCHED THEN
						UPDATE SET f_status_id = 2, f_date_updated = GETDATE()
					WHEN NOT MATCHED BY TARGET THEN
						INSERT (f_user_id, f_contact_id, f_status_id, f_date_created, f_date_updated)
						VALUES (Source.UserId, Source.ContactId, 2, GETDATE(), GETDATE());

					-- Return inserted contacts
					SELECT 
						f_contact_id AS ContactId,
						f_user_id AS UserId
					FROM Contacts
					WHERE f_user_id = @userId
						AND f_contact_id IN (SELECT UserId FROM @contactIds)
						AND f_status_id = 2;
				END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("DROP PROCEDURE [dbo].[spInsertContacts];");
        }
    }
}
