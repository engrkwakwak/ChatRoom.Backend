﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Repository;

#nullable disable

namespace ChatRoom.Backend.Migrations
{
    [DbContext(typeof(RepositoryContext))]
    [Migration("20240619010812_spGetUsers")]
    partial class spGetUsers
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Entities.Models.Chat", b =>
                {
                    b.Property<int>("ChatId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("f_chat_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ChatId"));

                    b.Property<string>("ChatName")
                        .HasColumnType("NVARCHAR(50)")
                        .HasColumnName("f_chat_name");

                    b.Property<int>("ChatTypeId")
                        .HasColumnType("int")
                        .HasColumnName("f_chat_type_id");

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2")
                        .HasColumnName("f_date_created");

                    b.Property<string>("DisplayPictureUrl")
                        .HasColumnType("VARCHAR(100)")
                        .HasColumnName("f_display_picture_url");

                    b.Property<int>("StatusId")
                        .HasColumnType("int")
                        .HasColumnName("f_status_id");

                    b.HasKey("ChatId");

                    b.HasIndex("ChatTypeId");

                    b.HasIndex("StatusId");

                    b.ToTable("Chats");
                });

            modelBuilder.Entity("Entities.Models.ChatMember", b =>
                {
                    b.Property<int>("ChatId")
                        .HasColumnType("int")
                        .HasColumnName("f_chat_id");

                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("f_user_id");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("bit")
                        .HasColumnName("f_is_admin");

                    b.Property<int>("LastSeenMessageId")
                        .HasColumnType("int")
                        .HasColumnName("f_last_seen_message_id");

                    b.Property<int>("StatusId")
                        .HasColumnType("int")
                        .HasColumnName("f_status_id");

                    b.HasKey("ChatId", "UserId");

                    b.HasIndex("LastSeenMessageId");

                    b.HasIndex("StatusId");

                    b.HasIndex("UserId");

                    b.ToTable("ChatMembers");
                });

            modelBuilder.Entity("Entities.Models.ChatType", b =>
                {
                    b.Property<int>("ChatTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("f_chat_type_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ChatTypeId"));

                    b.Property<string>("ChatTypeDescription")
                        .IsRequired()
                        .HasColumnType("VARCHAR(500)")
                        .HasColumnName("f_chat_type_description");

                    b.Property<string>("ChatTypeName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(15)")
                        .HasColumnName("f_chat_type_name");

                    b.HasKey("ChatTypeId");

                    b.ToTable("ChatTypes");

                    b.HasData(
                        new
                        {
                            ChatTypeId = 1,
                            ChatTypeDescription = "Peer-to-Peer (P2P) chat type is designed for direct, one-on-one communication between two users. This is similar to a private conversation where messages are only visible to the participating users.",
                            ChatTypeName = "P2P"
                        },
                        new
                        {
                            ChatTypeId = 2,
                            ChatTypeDescription = "Group Chat type is designed for multi-user conversations where messages can be seen by all members of the group. This is ideal for team discussions, project collaborations, or any scenario where multiple users need to communicate simultaneously.",
                            ChatTypeName = "GroupChat"
                        });
                });

            modelBuilder.Entity("Entities.Models.Contact", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("f_user_id");

                    b.Property<int>("ContactId")
                        .HasColumnType("int")
                        .HasColumnName("f_contact_id");

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2")
                        .HasColumnName("f_date_created");

                    b.Property<DateTime?>("DateUpdated")
                        .HasColumnType("datetime2")
                        .HasColumnName("f_date_updated");

                    b.Property<int>("StatusId")
                        .HasColumnType("int")
                        .HasColumnName("f_status_id");

                    b.HasKey("UserId", "ContactId");

                    b.HasIndex("ContactId");

                    b.HasIndex("StatusId");

                    b.ToTable("Contacts");
                });

            modelBuilder.Entity("Entities.Models.Message", b =>
                {
                    b.Property<int>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("f_message_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MessageId"));

                    b.Property<int>("ChatId")
                        .HasColumnType("int")
                        .HasColumnName("f_chat_id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("NVARCHAR(MAX)")
                        .HasColumnName("f_content");

                    b.Property<DateTime>("DateSent")
                        .HasColumnType("datetime2")
                        .HasColumnName("f_date_sent");

                    b.Property<DateTime>("DateUpdated")
                        .HasColumnType("datetime2")
                        .HasColumnName("f_date_updated");

                    b.Property<int>("MsgTypeId")
                        .HasColumnType("int")
                        .HasColumnName("f_msg_type_id");

                    b.Property<int>("SenderId")
                        .HasColumnType("int")
                        .HasColumnName("f_sender_id");

                    b.Property<int>("StatusId")
                        .HasColumnType("int")
                        .HasColumnName("f_status_id");

                    b.HasKey("MessageId");

                    b.HasIndex("ChatId");

                    b.HasIndex("MsgTypeId");

                    b.HasIndex("SenderId");

                    b.HasIndex("StatusId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Entities.Models.MessageType", b =>
                {
                    b.Property<int>("MsgTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("f_msg_type_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MsgTypeId"));

                    b.Property<string>("MsgTypeDescription")
                        .IsRequired()
                        .HasColumnType("VARCHAR(500)")
                        .HasColumnName("f_msg_type_description");

                    b.Property<string>("MsgTypeName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(15)")
                        .HasColumnName("f_msg_type_name");

                    b.HasKey("MsgTypeId");

                    b.ToTable("MessageTypes");

                    b.HasData(
                        new
                        {
                            MsgTypeId = 1,
                            MsgTypeDescription = "A 'Normal' message is a user-generated message, typically used for direct communication between users within a chat.",
                            MsgTypeName = "Normal"
                        },
                        new
                        {
                            MsgTypeId = 2,
                            MsgTypeDescription = "A 'Notification' message is a system-generated message, used to inform users about updates, events, or actions related to the chat or the application.",
                            MsgTypeName = "Notification"
                        });
                });

            modelBuilder.Entity("Entities.Models.Status", b =>
                {
                    b.Property<int>("StatusId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("f_status_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("StatusId"));

                    b.Property<string>("StatusName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(10)")
                        .HasColumnName("f_status_name");

                    b.HasKey("StatusId");

                    b.ToTable("Status");

                    b.HasData(
                        new
                        {
                            StatusId = 1,
                            StatusName = "Active"
                        },
                        new
                        {
                            StatusId = 2,
                            StatusName = "Approved"
                        },
                        new
                        {
                            StatusId = 3,
                            StatusName = "Deleted"
                        });
                });

            modelBuilder.Entity("Entities.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("f_user_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<string>("Address")
                        .HasColumnType("NVARCHAR(100)")
                        .HasColumnName("f_address");

                    b.Property<DateTime?>("BirthDate")
                        .HasColumnType("datetime2")
                        .HasColumnName("f_birthdate");

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2")
                        .HasColumnName("f_date_created");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("NVARCHAR(50)")
                        .HasColumnName("f_display_name");

                    b.Property<string>("DisplayPictureUrl")
                        .HasColumnType("VARCHAR(100)")
                        .HasColumnName("f_display_picture_url");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("VARCHAR(100)")
                        .HasColumnName("f_email");

                    b.Property<bool>("IsEmailVerified")
                        .HasColumnType("bit")
                        .HasColumnName("f_is_email_verified");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("VARCHAR(60)")
                        .HasColumnName("f_password_hash");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("NVARCHAR(20)")
                        .HasColumnName("f_username");

                    b.HasKey("UserId");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Entities.Models.Chat", b =>
                {
                    b.HasOne("Entities.Models.ChatType", "ChatType")
                        .WithMany("Chats")
                        .HasForeignKey("ChatTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Entities.Models.Status", "Status")
                        .WithMany("Chats")
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ChatType");

                    b.Navigation("Status");
                });

            modelBuilder.Entity("Entities.Models.ChatMember", b =>
                {
                    b.HasOne("Entities.Models.Chat", "Chat")
                        .WithMany("Members")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Entities.Models.Message", "Message")
                        .WithMany("LastSeenUsers")
                        .HasForeignKey("LastSeenMessageId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Entities.Models.Status", "Status")
                        .WithMany("ChatMembers")
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Entities.Models.User", "User")
                        .WithMany("ParticipatedChats")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Chat");

                    b.Navigation("Message");

                    b.Navigation("Status");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Entities.Models.Contact", b =>
                {
                    b.HasOne("Entities.Models.User", "UserContact")
                        .WithMany()
                        .HasForeignKey("ContactId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Entities.Models.Status", "Status")
                        .WithMany("Contacts")
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Entities.Models.User", "User")
                        .WithMany("Contacts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Status");

                    b.Navigation("User");

                    b.Navigation("UserContact");
                });

            modelBuilder.Entity("Entities.Models.Message", b =>
                {
                    b.HasOne("Entities.Models.Chat", "Chat")
                        .WithMany("Messages")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Entities.Models.MessageType", "MessageType")
                        .WithMany("Messages")
                        .HasForeignKey("MsgTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Entities.Models.User", "User")
                        .WithMany("Messages")
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Entities.Models.Status", "Status")
                        .WithMany("Messages")
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Chat");

                    b.Navigation("MessageType");

                    b.Navigation("Status");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Entities.Models.Chat", b =>
                {
                    b.Navigation("Members");

                    b.Navigation("Messages");
                });

            modelBuilder.Entity("Entities.Models.ChatType", b =>
                {
                    b.Navigation("Chats");
                });

            modelBuilder.Entity("Entities.Models.Message", b =>
                {
                    b.Navigation("LastSeenUsers");
                });

            modelBuilder.Entity("Entities.Models.MessageType", b =>
                {
                    b.Navigation("Messages");
                });

            modelBuilder.Entity("Entities.Models.Status", b =>
                {
                    b.Navigation("ChatMembers");

                    b.Navigation("Chats");

                    b.Navigation("Contacts");

                    b.Navigation("Messages");
                });

            modelBuilder.Entity("Entities.Models.User", b =>
                {
                    b.Navigation("Contacts");

                    b.Navigation("Messages");

                    b.Navigation("ParticipatedChats");
                });
#pragma warning restore 612, 618
        }
    }
}
