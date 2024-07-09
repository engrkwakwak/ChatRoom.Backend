using Shared.DataTransferObjects.Chats;
using Shared.Enums;
using StackExchange.Redis;

namespace ChatRoom.UnitTest.Helpers
{
    public class ChatDtoFactory
    {
        public static ChatDto CreateP2PChatDto()
        {
            return new ChatDto()
            {
                ChatId = 1,
                ChatTypeId = (int)ChatTypes.P2P,
                StatusId = 1,
                Members = [
                    new ChatMemberDto{
                        ChatId = 1,
                        IsAdmin = false,
                        StatusId=1,
                        UserId = 1,
                        User = new() {
                            DisplayName = "Test Name 1",
                            UserId=1
                        },
                    },
                    new ChatMemberDto{
                        ChatId = 1,
                        IsAdmin = false,
                        StatusId=1,
                        UserId = 2,
                        User = new() {
                            DisplayName = "Test Name 2",
                            UserId=2
                        },
                    }
                ]
            };
        }
        public static ChatDto CreateGroupChatDto(int chatId = 1)
        {
            return new ChatDto()
            {
                ChatId = chatId,
                ChatTypeId = (int)ChatTypes.GroupChat,
                StatusId = 1,
                ChatName = "Test Group Chat",
                DisplayPictureUrl = null,
                Members = [
                    new ChatMemberDto{
                        ChatId = chatId,
                        IsAdmin = true,
                        StatusId=1,
                        UserId = 1,
                        User = new() {
                            DisplayName = "Test Name 1",
                            UserId=1
                        },
                    },
                    new ChatMemberDto{
                        ChatId = chatId,
                        IsAdmin = false,
                        StatusId=1,
                        UserId = 2,
                        User = new() {
                            DisplayName = "Test Name 2",
                            UserId=1
                        },
                    }
                ]
            };
        }
        public static ChatDto CreateGroupChatDto(string displayPictureUrl)
        {
            return new ChatDto()
            {
                ChatId = 1,
                ChatTypeId = (int)ChatTypes.GroupChat,
                StatusId = 1,
                ChatName = "Test Group Chat",
                DisplayPictureUrl = displayPictureUrl,
                Members = [
                    new ChatMemberDto{
                        ChatId = 1,
                        IsAdmin = true,
                        StatusId=1,
                        UserId = 1,
                        User = new() {
                            DisplayName = "Test Name 1",
                            UserId=1
                        },
                    },
                    new ChatMemberDto{
                        ChatId = 1,
                        IsAdmin = false,
                        StatusId=1,
                        UserId = 2,
                        User = new() {
                            DisplayName = "Test Name 2",
                            UserId=1
                        },
                    }
                ]
            };
        }

        public static ChatForCreationDto CreateValidP2PChatForCreationDto()
        {
            return new ChatForCreationDto
            {
                ChatTypeId = (int)ChatTypes.P2P,
                StatusId = 1,
                ChatMemberIds = [1,2]
            };
        }


        /// <summary>
        /// Returns invlaid ChatForCreationDto model with ChatTypeId = 1 and ChatMembers = [0,0] to make it invalid
        /// </summary>
        /// <returns>ChatForCreationDto</returns>
        public static ChatForCreationDto CreateInValidP2PChatForCreationDto()
        {
            return new ChatForCreationDto
            {
                ChatTypeId = 0,
                StatusId = 1,
                ChatMemberIds = [0,0]
            };
        }

        /// <summary>
        /// Returns invlaid ChatForCreationDto model with ChatTypeId = 1 and ChatMembers = [0,0] to make it invalid
        /// </summary>
        /// <returns>ChatForCreationDto</returns>
        public static ChatForCreationDto CreateInValidGroupChatForCreationDto()
        {
            return new ChatForCreationDto
            {
                ChatTypeId = 0,
                StatusId = 2,
                ChatMemberIds = [0,0],
                ChatName = "Sed ut perspiciatis unde omnis iste natus error sit"
            };
        }        
        
        public static ChatForCreationDto CreateValidGroupChatForCreationDto()
        {
            return new ChatForCreationDto
            {
                ChatTypeId = 2,
                StatusId = 2,
                ChatMemberIds = [1,2],
                ChatName = "Sed ut perspiciatis unde omnis iste natus error sit"
            };
        }

        public static IEnumerable<ChatDto> CreateChatDtos()
        {
            return [
                    ChatDtoFactory.CreateP2PChatDto(),
                    ChatDtoFactory.CreateP2PChatDto(),
                    ChatDtoFactory.CreateP2PChatDto(),
                ];
        }

        public static ChatForUpdateDto CreateChatForUpdateDto()
        {
            return new()
            {
                ChatName = "Test Chat Name",
                DisplayPictureUrl = "test-display-url"
            };
        }
    }
}
