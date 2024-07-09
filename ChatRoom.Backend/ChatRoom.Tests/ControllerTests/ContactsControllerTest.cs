
using ChatRoom.Backend.Presentation.Controllers;
using ChatRoom.Backend.Presentation.Hubs;
using ChatRoom.UnitTest.Helpers;
using Contracts;
using Entities.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Org.BouncyCastle.Crypto;
using Service.Contracts;
using Shared.DataTransferObjects.Contacts;
using Shared.DataTransferObjects.Status;
using Shared.DataTransferObjects.Users;
using Shared.RequestFeatures;

namespace ChatRoom.UnitTest.ControllerTests
{
    public class ContactsControllerTest
    {
        private readonly Mock<IServiceManager> _serviceMock = new();

        private readonly Mock<IHubContext<ChatRoomHub>> _hubContextMock = new();

        private readonly Mock<IFileManager> _fileManagerMock = new();

        private readonly ContactsController _controller;

        public ContactsControllerTest()
        {
            _controller = new ContactsController(_serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }


        [Fact]
        public async Task Create_ContactIdAndUserIdIsEqual_ThrowsInvalidParameterException()
        {
            // arrange
            ContactForCreationDto contactForCreationDto = ContactDtoFactory.CreateContactForCreationDto(contactId: 1);

            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.Create(contactForCreationDto));

            // assert
            Assert.Equal("Something went wrong. The request parameters are invalid", result.Message);
        }

        [Fact]
        public async Task Create_UserDoesNotExist_ThrowsInvalidParameterException()
        {
            // arrange
            ContactForCreationDto contactForCreationDto = ContactDtoFactory.CreateContactForCreationDto();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).Verifiable();
            
            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.Create(contactForCreationDto));

            // assert
            _serviceMock.Verify();
            Assert.Equal("Something went wrong. Record doesnt exist", result.Message);

        }

        [Fact]
        public async Task Create_ContactUserDoesNotExist_ThrowsInvalidParameterException()
        {
            // arrange
            ContactForCreationDto contactForCreationDto = ContactDtoFactory.CreateContactForCreationDto();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync((int)contactForCreationDto.UserId!)).ReturnsAsync(UserDtoFactory.CreateUserDto()).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync((int)contactForCreationDto.ContactId!)).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.Create(contactForCreationDto));

            // assert
            _serviceMock.Verify();
            Assert.Equal("Something went wrong. Record doesnt exist", result.Message);
        }

        [Fact]
        public async Task Create_StatusDoesNotExist_ThrowsInvalidParameterException()
        {
            // arrange
            ContactForCreationDto contactForCreationDto = ContactDtoFactory.CreateContactForCreationDto();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync((int)contactForCreationDto.UserId!)).ReturnsAsync(UserDtoFactory.CreateUserDto()).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync((int)contactForCreationDto.ContactId!)).ReturnsAsync(UserDtoFactory.CreateUserDto()).Verifiable();
            _serviceMock.Setup(s => s.StatusService.GetStatusByIdAsync((int)contactForCreationDto.StatusId!)).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.Create(contactForCreationDto));

            // assert
            _serviceMock.Verify();
            Assert.Equal("Something went wrong. Record doesnt exist", result.Message);
        }

        [Fact]
        public async Task Create_CreatingContactFailed_ThrowsException()
        {
            // arrange
            StatusDto statusDto = new();
            ContactForCreationDto contactForCreationDto = ContactDtoFactory.CreateContactForCreationDto();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync((int)contactForCreationDto.UserId!)).ReturnsAsync(UserDtoFactory.CreateUserDto()).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync((int)contactForCreationDto.ContactId!)).ReturnsAsync(UserDtoFactory.CreateUserDto()).Verifiable();
            _serviceMock.Setup(s => s.StatusService.GetStatusByIdAsync((int)contactForCreationDto.StatusId!)).ReturnsAsync(statusDto).Verifiable();
            _serviceMock.Setup(s => s.ContactService.InsertOrUpdateContactAsync(contactForCreationDto)).ReturnsAsync(false).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<Exception>(async () => await _controller.Create(contactForCreationDto));

            // assert
            _serviceMock.Verify();
            Assert.Equal("Something went wrong while processing the request.", result.Message);
        }

        [Fact]
        public async Task Create_CreatingContactSucceeds_ReturnsCreatedResult()
        {
            // arrange
            StatusDto statusDto = new();
            ContactForCreationDto contactForCreationDto = ContactDtoFactory.CreateContactForCreationDto();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync((int)contactForCreationDto.UserId!)).ReturnsAsync(UserDtoFactory.CreateUserDto()).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync((int)contactForCreationDto.ContactId!)).ReturnsAsync(UserDtoFactory.CreateUserDto()).Verifiable();
            _serviceMock.Setup(s => s.StatusService.GetStatusByIdAsync((int)contactForCreationDto.StatusId!)).ReturnsAsync(statusDto).Verifiable();
            _serviceMock.Setup(s => s.ContactService.InsertOrUpdateContactAsync(contactForCreationDto)).ReturnsAsync(true).Verifiable();

            // act
            var result = await _controller.Create(contactForCreationDto);

            // assert
            _serviceMock.Verify();
            Assert.IsType<CreatedResult>(result);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        public async Task Delete_UserIdOrContactIdIsLessThanOne_ThrowsInvalidParameterException(int userId, int contactId)
        {
            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.Delete(userId, contactId));

            // assert
            Assert.NotNull(result);
            Assert.Equal("The request parameters are invalid", result.Message);
        }

        [Fact]
        public async Task Delete_DeletingContactFailed_ThrowsException()
        {
            // arrange
            int userId = 1, contactId = 1;
            _serviceMock.Setup(s => s.ContactService.DeleteContactByUserIdContactIdAsync(userId, contactId)).ReturnsAsync(false).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<Exception>(async () => await _controller.Delete(userId, contactId));

            // assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.Equal("Something went wrong while deleting the contact.", result.Message);
        }

        [Fact]
        public async Task Delete_DeletingContactSucceeds_ReturnsOkResult()
        {
            // arrange
            int userId = 1, contactId = 1;
            _serviceMock.Setup(s => s.ContactService.DeleteContactByUserIdContactIdAsync(userId, contactId)).ReturnsAsync(true).Verifiable();

            // act
            var result = await _controller.Delete(userId, contactId);

            // assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ViewContacts_UserIdLessThanOne_ThrowsInvalidParameterException()
        {
            // arrange
            ContactParameters contactParameters = new()
            {
                Name = "name",
                UserId = 0,
            };

            // act 
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.ViewContacts(contactParameters));

            // assert
            Assert.NotNull(result);
            Assert.Equal("The request parameters are invalid", result.Message);
        }

        [Fact]
        public async Task ViewContacts_Success_ReturnOkObjectResult()
        {
            // arrange
            ContactParameters contactParameters = new()
            {
                Name = "name",
                UserId = 1,
            };
            _serviceMock.Setup(s => s.ContactService.SearchContactsByNameUserIdAsync(contactParameters)).ReturnsAsync(UserDtoFactory.CreateListOfUserDto()).Verifiable();

            // act 
            var result = await _controller.ViewContacts(contactParameters);

            // assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.IsAssignableFrom<IEnumerable<UserDto>>(((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetActiveContactInfo_ContactIsNull_ReturnsOkObjectResultWithNullValue()
        {
            // arrange
            int userId = 1, contactId = 1;
            _serviceMock.Setup(s => s.ContactService.GetContactByUserIdContactIdAsync(userId, contactId)).Verifiable();

            // act 
            var result = await _controller.GetActiveContactInfo(userId, contactId);

            // assert
            _serviceMock.Verify();
            Assert.IsType<OkObjectResult>(result);
            Assert.Null(((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetActiveContactInfo_ContactStatusIsNotApproved_ReturnsOkObjectResultWithNullValue()
        {
            // arrange
            int userId = 1, contactId = 1;
            _serviceMock.Setup(s => s.ContactService.GetContactByUserIdContactIdAsync(userId, contactId)).ReturnsAsync(ContactDtoFactory.CreateContactDto(statusId:3)).Verifiable();

            // act 
            var result = await _controller.GetActiveContactInfo(userId, contactId);

            // assert
            _serviceMock.Verify();
            Assert.IsType<OkObjectResult>(result);
            Assert.Null(((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetActiveContactInfo_ContactIsNull_ReturnsOkObjectResultWithContactValue()
        {
            // arrange
            int userId = 1, contactId = 1;
            _serviceMock.Setup(s => s.ContactService.GetContactByUserIdContactIdAsync(userId, contactId)).ReturnsAsync(ContactDtoFactory.CreateContactDto()).Verifiable();

            // act 
            var result = await _controller.GetActiveContactInfo(userId, contactId);

            // assert
            _serviceMock.Verify();
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ContactDto>(((OkObjectResult)result).Value);
        }
    }
}
