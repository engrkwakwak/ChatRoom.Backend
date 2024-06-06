using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class tvpUserIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TYPE [dbo].[tvpUserIds] AS TABLE
                (
                    [UserId] int NOT NULL
                )
            ");     
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TYPE [dbo].[tvpUserIds]");
        }
    }
}
