using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spGetStatusById : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spGetStatusById]
                    @StatusId int
                AS
                BEGIN
                    SELECT 
	                    f_status_id AS StatusId,
	                    f_status_name AS StatusName
                    FROM [Status] WITH (NOLOCK)
                    WHERE f_status_id=@StatusId;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetStatusById]");
        }
    }
}
