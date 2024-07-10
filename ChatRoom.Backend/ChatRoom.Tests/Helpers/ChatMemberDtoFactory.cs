using Entities.Exceptions;
using Entities.Models;
using Shared.DataTransferObjects.Chats;
using System.Runtime.CompilerServices;
namespace ChatRoom.UnitTest.Helpers
{
    public class ChatMemberDtoFactory
    {
        public static IEnumerable<ChatMemberDto> CreateChatMembersList()
        {
            return [
                new ChatMemberDto{
                    ChatId = 1,
                    StatusId = 1,
                    UserId = 1,
                    IsAdmin = true,
                    User = new(){
                        UserId = 1,
                        DisplayName = "test name",
                    }
                },
                new ChatMemberDto{
                    ChatId = 1,
                    StatusId = 1,
                    UserId = 2,
                    IsAdmin = false,
                    User = new(){
                        UserId = 2,
                        DisplayName = "test name",
                    }
                }
            ];
        }        
        
        public static IEnumerable<ChatMemberDto> CreateChatMembersListWithMultipleAdmin()
        {
            return [
                new ChatMemberDto{
                    ChatId = 1,
                    StatusId = 1,
                    UserId = 1,
                    IsAdmin = true,
                    User = new(){
                        UserId = 1,
                        DisplayName = "test name",
                    }
                },
                new ChatMemberDto{
                    ChatId = 1,
                    StatusId = 1,
                    UserId = 2,
                    IsAdmin = false,
                    User = new(){
                        UserId = 2,
                        DisplayName = "test name",
                    }
                },
                new ChatMemberDto{
                    ChatId = 1,
                    StatusId = 1,
                    UserId = 3,
                    IsAdmin = true,
                    User = new(){
                        UserId = 3,
                        DisplayName = "test name",
                    }
                }
            ];
        }   
        
        public static ChatMemberDto CreateChatMemberDto(bool isAdmin = false, int userId = 1, int chatId=1)
        {
            return new ChatMemberDto
            {
                ChatId = chatId,
                StatusId = 1,
                UserId = userId,
                IsAdmin = isAdmin,
            };
        }

        public static ChatMemberForUpdateDto CreateChatMemberForUpdateDto(
            bool isAdmin=true, 
            int lastSeenMessageId=5, 
            int statusId=1)
        {
            return new()
            {
                IsAdmin = isAdmin,
                LastSeenMessageId = lastSeenMessageId,
                StatusId = statusId
            };
        }

        public static ChatMember CreateChatMember(
            int chatId = 1,
            bool isAdmin = true,
            int lastSeenMessageId = 1,
            int statusId = 1,
            int userId = 1,
            User? user=null
            )
        {
            return new()
            {
                ChatId = chatId,
                IsAdmin = isAdmin,
                LastSeenMessageId = lastSeenMessageId,
                StatusId = statusId,
                UserId = userId,
                User = user ?? UserDtoFactory.CreateUser(userId: userId)
            };
        }

        
    }
}
