using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatTypes",
                columns: table => new
                {
                    f_chat_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    f_chat_type_name = table.Column<string>(type: "VARCHAR(15)", nullable: false),
                    f_chat_type_description = table.Column<string>(type: "VARCHAR(500)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatTypes", x => x.f_chat_type_id);
                });

            migrationBuilder.CreateTable(
                name: "MessageTypes",
                columns: table => new
                {
                    f_msg_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    f_msg_type_name = table.Column<string>(type: "VARCHAR(15)", nullable: false),
                    f_msg_type_description = table.Column<string>(type: "VARCHAR(500)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTypes", x => x.f_msg_type_id);
                });

            migrationBuilder.CreateTable(
                name: "Status",
                columns: table => new
                {
                    f_status_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    f_status_name = table.Column<string>(type: "VARCHAR(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.f_status_id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    f_user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    f_username = table.Column<string>(type: "NVARCHAR(20)", nullable: false),
                    f_display_name = table.Column<string>(type: "NVARCHAR(50)", nullable: false),
                    f_email = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    f_password_hash = table.Column<string>(type: "VARCHAR(60)", nullable: false),
                    f_address = table.Column<string>(type: "NVARCHAR(100)", nullable: true),
                    f_birthdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    f_is_email_verified = table.Column<bool>(type: "bit", nullable: false),
                    f_display_picture_url = table.Column<string>(type: "VARCHAR(100)", nullable: true),
                    f_date_created = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.f_user_id);
                });

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    f_chat_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    f_chat_type_id = table.Column<int>(type: "int", nullable: false),
                    f_chat_name = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    f_display_picture_url = table.Column<string>(type: "VARCHAR(100)", nullable: true),
                    f_date_created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    f_status_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.f_chat_id);
                    table.ForeignKey(
                        name: "FK_Chats_ChatTypes_f_chat_type_id",
                        column: x => x.f_chat_type_id,
                        principalTable: "ChatTypes",
                        principalColumn: "f_chat_type_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chats_Status_f_status_id",
                        column: x => x.f_status_id,
                        principalTable: "Status",
                        principalColumn: "f_status_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    f_user_id = table.Column<int>(type: "int", nullable: false),
                    f_contact_id = table.Column<int>(type: "int", nullable: false),
                    f_status_id = table.Column<int>(type: "int", nullable: false),
                    f_date_created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    f_date_updated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => new { x.f_user_id, x.f_contact_id });
                    table.ForeignKey(
                        name: "FK_Contacts_Status_f_status_id",
                        column: x => x.f_status_id,
                        principalTable: "Status",
                        principalColumn: "f_status_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contacts_Users_f_contact_id",
                        column: x => x.f_contact_id,
                        principalTable: "Users",
                        principalColumn: "f_user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contacts_Users_f_user_id",
                        column: x => x.f_user_id,
                        principalTable: "Users",
                        principalColumn: "f_user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    f_message_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    f_chat_id = table.Column<int>(type: "int", nullable: false),
                    f_sender_id = table.Column<int>(type: "int", nullable: false),
                    f_msg_type_id = table.Column<int>(type: "int", nullable: false),
                    f_content = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    f_date_sent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    f_date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    f_status_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.f_message_id);
                    table.ForeignKey(
                        name: "FK_Messages_Chats_f_chat_id",
                        column: x => x.f_chat_id,
                        principalTable: "Chats",
                        principalColumn: "f_chat_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_MessageTypes_f_msg_type_id",
                        column: x => x.f_msg_type_id,
                        principalTable: "MessageTypes",
                        principalColumn: "f_msg_type_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Status_f_status_id",
                        column: x => x.f_status_id,
                        principalTable: "Status",
                        principalColumn: "f_status_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Users_f_sender_id",
                        column: x => x.f_sender_id,
                        principalTable: "Users",
                        principalColumn: "f_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMembers",
                columns: table => new
                {
                    f_chat_id = table.Column<int>(type: "int", nullable: false),
                    f_user_id = table.Column<int>(type: "int", nullable: false),
                    f_is_admin = table.Column<bool>(type: "bit", nullable: false),
                    f_last_seen_message_id = table.Column<int>(type: "int", nullable: false),
                    f_status_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMembers", x => new { x.f_chat_id, x.f_user_id });
                    table.ForeignKey(
                        name: "FK_ChatMembers_Chats_f_chat_id",
                        column: x => x.f_chat_id,
                        principalTable: "Chats",
                        principalColumn: "f_chat_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMembers_Messages_f_last_seen_message_id",
                        column: x => x.f_last_seen_message_id,
                        principalTable: "Messages",
                        principalColumn: "f_message_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMembers_Status_f_status_id",
                        column: x => x.f_status_id,
                        principalTable: "Status",
                        principalColumn: "f_status_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMembers_Users_f_user_id",
                        column: x => x.f_user_id,
                        principalTable: "Users",
                        principalColumn: "f_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMembers_f_last_seen_message_id",
                table: "ChatMembers",
                column: "f_last_seen_message_id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMembers_f_status_id",
                table: "ChatMembers",
                column: "f_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMembers_f_user_id",
                table: "ChatMembers",
                column: "f_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_f_chat_type_id",
                table: "Chats",
                column: "f_chat_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_f_status_id",
                table: "Chats",
                column: "f_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_f_contact_id",
                table: "Contacts",
                column: "f_contact_id");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_f_status_id",
                table: "Contacts",
                column: "f_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_f_chat_id",
                table: "Messages",
                column: "f_chat_id");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_f_msg_type_id",
                table: "Messages",
                column: "f_msg_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_f_sender_id",
                table: "Messages",
                column: "f_sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_f_status_id",
                table: "Messages",
                column: "f_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_f_username",
                table: "Users",
                column: "f_username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMembers");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropTable(
                name: "MessageTypes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ChatTypes");

            migrationBuilder.DropTable(
                name: "Status");
        }
    }
}
