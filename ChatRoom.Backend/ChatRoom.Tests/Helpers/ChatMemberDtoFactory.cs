using Entities.Exceptions;
using Entities.Models;
using Shared.DataTransferObjects.Chats;
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
        
        public static ChatMemberDto CreateChatMemberDto(bool isAdmin = false, int userId = 1)
        {
            return new ChatMemberDto
            {
                ChatId = 1,
                StatusId = 1,
                UserId = userId,
                IsAdmin = isAdmin,
            };
        }
    }
}
