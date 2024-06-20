using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spSetAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spSetAdmin]
                    @ChatId int,
                    @UserId int,
                    @IsAdmin bit
                AS 
                BEGIN 
                   UPDATE ChatMembers WITH (ROWLOCK)
                   SET [f_is_admin]=@IsAdmin
                   WHERE [f_chat_id]=@ChatId
                        AND [f_user_id]=@UserId
                        AND [f_status_id] NOT IN (3)
                END     
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spSetAdmin]");
        }
    }
}
