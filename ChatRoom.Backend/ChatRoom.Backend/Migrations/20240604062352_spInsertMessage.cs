﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    /// <inheritdoc />
    public partial class spInsertMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[spInsertMessage]
                    @ChatId int,
	                @SenderId int,
	                @Content nvarchar(MAX),
	                @MessageTypeId int
                AS
                BEGIN
					DECLARE @InsertedMessages TABLE (
						MessageId int,
						ChatId int,
						SenderId int,
						Content nvarchar(MAX),
						MsgTypeId int,
						DateSent datetime,
						DateUpdated datetime,
						StatusId int
					);

                    INSERT INTO [Messages]
	                    (f_chat_id, f_sender_id, f_content, f_msg_type_id,  f_date_sent, f_date_updated, f_status_id)
					OUTPUT
						inserted.f_message_id as MessageId,
						inserted.f_chat_id as ChatId,
						inserted.f_sender_id as SenderId,
						inserted.f_content as Content,
						inserted.f_msg_type_id as MsgTypeId,
						inserted.f_date_sent as DateSent,
						inserted.f_date_updated as DateUpdated,
						inserted.f_status_id as StatusId
					INTO @InsertedMessages
                    VALUES
	                    (@ChatId, @SenderId, @Content, @MessageTypeId, GETDATE(), GETDATE(), 1);

					SELECT
						MessageId,
						ChatId,
						Content,
						DateSent,
						SenderId AS UserId,
						u.f_display_name AS DisplayName,
						u.f_display_picture_url AS DisplayPictureUrl,
						im.MsgTypeId,
						mt.f_msg_type_name AS MsgTypeName,
						im.StatusId,
						s.f_status_name AS StatusName
					FROM @InsertedMessages im
					INNER JOIN Users u
						ON im.SenderId = u.f_user_id
					INNER JOIN MessageTypes mt
						ON im.MsgTypeId = mt.f_msg_type_id
					INNER JOIN [Status] s
						ON im.StatusId = s.f_status_id;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spInsertMessage]");
        }
    }
}
