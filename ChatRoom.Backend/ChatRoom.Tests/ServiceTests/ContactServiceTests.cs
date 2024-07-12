using AutoMapper;
using ChatRoom.UnitTest.Helpers;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using RedisCacheService;
using Service;
using Service.Contracts;
using Shared.DataTransferObjects.Contacts;
using Shared.DataTransferObjects.Users;
using Shared.RequestFeatures;

namespace ChatRoom.UnitTest.ServiceTests
{
    public class ContactServiceTests
    {
        private readonly Mock<IRepositoryManager> _repositoryMock = new();
        private readonly Mock<ILoggerManager> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly Mock<IRedisCacheManager> _cacheMock = new();
        private readonly Mock<IFileManager> _fileManagerMock = new();
        private readonly Mock<ISmtpClientManager> _smtpClientMock = new();
        private readonly IServiceManager _serviceManager;

        public ContactServiceTests()
        {
            _serviceManager = new ServiceManager(_repositoryMock.Object, _loggerMock.Object, _mapperMock.Object, _configurationMock.Object, _cacheMock.Object, _fileManagerMock.Object, _smtpClientMock.Object);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        public async Task DeleteContactByUserIdContactIdAsync_UserIdOrContactIdIsLessThanOneOrEquals_ThrowsInvalidParameterException(int userId, int contactId)
        {
            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ContactService.DeleteContactByUserIdContactIdAsync(userId, contactId));

            // assert
            Assert.NotNull(result);
            Assert.Equal($"Invalid parameter user id {userId} and contact id {contactId}.", result.Message);
        }

        [Fact]
        public async Task DeleteContactByUserIdContactIdAsync_HasAffectedRows_ReturnsTrue()
        {
            // arrange
            int userId = 1, contactId = 2;
            _repositoryMock.Setup(r => r.Contact.DeleteContactByUserIdContactIdAsync(userId, contactId)).ReturnsAsync(1).Verifiable();
            _cacheMock.Setup(c => c.RemoveDataAsync(It.IsAny<string>())).Verifiable(Times.Exactly(2));

            // act
            var result = await _serviceManager.ContactService.DeleteContactByUserIdContactIdAsync(userId, contactId);

            // assert
            _repositoryMock.Verify();
            _cacheMock.Verify();
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteContactByUserIdContactIdAsync_HasNoAffectedRows_ReturnsFalse()
        {
            // arrange
            int userId = 1, contactId = 2;
            _repositoryMock.Setup(r => r.Contact.DeleteContactByUserIdContactIdAsync(userId, contactId)).ReturnsAsync(0).Verifiable();

            // act
            var result = await _serviceManager.ContactService.DeleteContactByUserIdContactIdAsync(userId, contactId);

            // assert
            _repositoryMock.Verify();
            Assert.False(result);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        public async Task GetContactByUserIdContactIdAsync_UserIdAndContactIdIsSameOrLessThanOne_ThrowsInvalidParameterException(int userId, int contactId)
        {
            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ContactService.GetContactByUserIdContactIdAsync(userId, contactId));

            // assert
            Assert.Equal($"Invalid parameter user id {userId} and contact id {contactId}.", result.Message);
        }

        [Fact]
        public async Task GetContactByUserIdContactIdAsync_ContactIsAvailableOnCache_ReturnsContactDto()
        {
            // arrange
            int userId = 1, contactId = 2;
            Contact contact = ContactDtoFactory.CreateContact(userId: userId, contactId: contactId);
            ContactDto contactDto = ContactDtoFactory.CreateContactDto(userId: userId, contactId: contactId);
            _cacheMock.Setup(c => c.GetCachedDataAsync<Contact>(It.IsAny<string>())).ReturnsAsync(contact).Verifiable();
            _mapperMock.Setup(m => m.Map<ContactDto>(It.IsAny<Contact>())).Returns(contactDto).Verifiable();

            // act
            var result = await _serviceManager.ContactService.GetContactByUserIdContactIdAsync(userId, contactId);

            // assert
            _cacheMock.Verify();
            _mapperMock.Verify();
            Assert.IsType<ContactDto>(result);
            Assert.Equivalent(contactDto, result);
        }

        [Fact]
        public async Task GetContactByUserIdContactIdAsync_ContactIsNull_ReturnsNull()
        {
            // arrange
            int userId = 1, contactId = 2;
            Contact contact = ContactDtoFactory.CreateContact(userId: userId, contactId: contactId);
            ContactDto contactDto = ContactDtoFactory.CreateContactDto(userId: userId, contactId: contactId);
            _cacheMock.Setup(c => c.GetCachedDataAsync<Contact>(It.IsAny<string>())).Verifiable();
            _repositoryMock.Setup(r => r.Contact.GetContactByUserIdContactIdAsync(userId, contactId)).Verifiable();

            // act
            var result = await _serviceManager.ContactService.GetContactByUserIdContactIdAsync(userId, contactId);

            // assert
            _cacheMock.Verify();
            _repositoryMock.Verify();
            Assert.Null(result);
        }

        [Fact]
        public async Task GetContactByUserIdContactIdAsync_ContactIsNotNull_ReturnsContactDto()
        {
            // arrange
            int userId = 1, contactId = 2;
            Contact contact = ContactDtoFactory.CreateContact(userId: userId, contactId: contactId);
            ContactDto contactDto = ContactDtoFactory.CreateContactDto(userId: userId, contactId: contactId);
            _cacheMock.Setup(c => c.GetCachedDataAsync<Contact>(It.IsAny<string>())).Verifiable();
            _repositoryMock.Setup(r => r.Contact.GetContactByUserIdContactIdAsync(userId, contactId)).ReturnsAsync(contact).Verifiable();
            _mapperMock.Setup(m => m.Map<ContactDto>(It.IsAny<Contact>())).Returns(contactDto).Verifiable(Times.Once);

            // act
            var result = await _serviceManager.ContactService.GetContactByUserIdContactIdAsync(userId, contactId);

            // assert
            _cacheMock.Verify();
            _mapperMock.Verify();
            _repositoryMock.Verify();
            Assert.IsType<ContactDto>(result);
            Assert.Equivalent(contactDto, result);
        }

        [Fact]
        public async Task GetContactsByUserIdAsync_ContactsAreNotCachedOrEmptyAndPageIsOne_ReturnsContactsList()
        {
            // arrange
            ContactParameters contactParams = new() { UserId = 1, Name = "Test", PageNumber = 1 };
            IEnumerable<Contact> contacts = [
                ContactDtoFactory.CreateContact(userId: 2),
                ContactDtoFactory.CreateContact(userId: 3),
                ContactDtoFactory.CreateContact(userId: 4),
                ];
            IEnumerable<ContactDto> contactDtos = [
                ContactDtoFactory.CreateContactDto(userId: 2),
                ContactDtoFactory.CreateContactDto(userId: 3),
                ContactDtoFactory.CreateContactDto(userId: 4),
                ];
            _cacheMock.Setup(c => c.GetCachedDataAsync<IEnumerable<Contact>>(It.IsAny<string>())).Verifiable();
            _repositoryMock.Setup(r => r.Contact.GetContactsByUserIdAsync(It.IsAny<ContactParameters>())).ReturnsAsync(contacts).Verifiable();
            _mapperMock.Setup(m => m.Map<IEnumerable<ContactDto>>(It.IsAny<IEnumerable<Contact>>())).Returns(contactDtos).Verifiable(Times.Once);
            _cacheMock.Setup(c => c.SetCachedDataAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Contact>>(), TimeSpan.FromMinutes(30))).Verifiable();

            // act
            var result = await _serviceManager.ContactService.GetContactsByUserIdAsync(contactParams);

            // assert
            _cacheMock.Verify();
            _repositoryMock.Verify();
            _mapperMock.Verify();
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<ContactDto>>(result);
        }

        [Fact]
        public async Task GetContactsByUserIdAsync_ContactsAreOnCachedAndPageIsOne_ReturnsContactsList()
        {
            // arrange
            ContactParameters contactParams = new() { UserId = 1, Name = "Test", PageNumber = 1 };
            IEnumerable<Contact> contacts = [
                ContactDtoFactory.CreateContact(userId: 2),
                ContactDtoFactory.CreateContact(userId: 3),
                ContactDtoFactory.CreateContact(userId: 4),
                ];
            IEnumerable<ContactDto> contactDtos = [
                ContactDtoFactory.CreateContactDto(userId: 2),
                ContactDtoFactory.CreateContactDto(userId: 3),
                ContactDtoFactory.CreateContactDto(userId: 4),
                ];
            _cacheMock.Setup(c => c.GetCachedDataAsync<IEnumerable<Contact>>(It.IsAny<string>())).ReturnsAsync(contacts).Verifiable();
            _mapperMock.Setup(m => m.Map<IEnumerable<ContactDto>>(It.IsAny<IEnumerable<Contact>>())).Returns(contactDtos).Verifiable(Times.Once);

            // act
            var result = await _serviceManager.ContactService.GetContactsByUserIdAsync(contactParams);

            // assert
            _cacheMock.Verify();
            _mapperMock.Verify();
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<ContactDto>>(result);
        }

        [Fact]
        public async Task GetContactsByUserIdAsync_Default_ReturnsContactsList()
        {
            // arrange
            ContactParameters contactParams = new() { UserId = 1, Name = "Test", PageNumber = 2 };
            IEnumerable<Contact> contacts = [
                ContactDtoFactory.CreateContact(userId: 2),
                ContactDtoFactory.CreateContact(userId: 3),
                ContactDtoFactory.CreateContact(userId: 4),
                ];
            IEnumerable<ContactDto> contactDtos = [
                ContactDtoFactory.CreateContactDto(userId: 2),
                ContactDtoFactory.CreateContactDto(userId: 3),
                ContactDtoFactory.CreateContactDto(userId: 4),
                ];
            _cacheMock.Setup(c => c.GetCachedDataAsync<IEnumerable<Contact>>(It.IsAny<string>())).Verifiable();
            _repositoryMock.Setup(r => r.Contact.GetContactsByUserIdAsync(It.IsAny<ContactParameters>())).ReturnsAsync(contacts).Verifiable();
            _mapperMock.Setup(m => m.Map<IEnumerable<ContactDto>>(It.IsAny<IEnumerable<Contact>>())).Returns(contactDtos).Verifiable(Times.Once);

            // act
            var result = await _serviceManager.ContactService.GetContactsByUserIdAsync(contactParams);

            // assert
            _cacheMock.Verify();
            _repositoryMock.Verify();
            _mapperMock.Verify();
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<ContactDto>>(result);
        }

        [Fact]
        public async Task GetContactsByUserIdAsync_DefaultAndPageIsOne_ReturnsContactsList()
        {
            // arrange
            ContactParameters contactParams = new() { UserId = 1, Name = "Test", PageNumber = 1 };
            IEnumerable<Contact> contacts = [
                ContactDtoFactory.CreateContact(userId: 2),
                ContactDtoFactory.CreateContact(userId: 3),
                ContactDtoFactory.CreateContact(userId: 4),
                ];
            IEnumerable<ContactDto> contactDtos = [
                ContactDtoFactory.CreateContactDto(userId: 2),
                ContactDtoFactory.CreateContactDto(userId: 3),
                ContactDtoFactory.CreateContactDto(userId: 4),
                ];
            _cacheMock.Setup(c => c.GetCachedDataAsync<IEnumerable<Contact>>(It.IsAny<string>())).Verifiable();
            _repositoryMock.Setup(r => r.Contact.GetContactsByUserIdAsync(It.IsAny<ContactParameters>())).ReturnsAsync(contacts).Verifiable();
            _mapperMock.Setup(m => m.Map<IEnumerable<ContactDto>>(It.IsAny<IEnumerable<Contact>>())).Returns(contactDtos).Verifiable(Times.Once);
            _cacheMock.Setup(c => c.SetCachedDataAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Contact>>(), TimeSpan.FromMinutes(30))).Verifiable();

            // act
            var result = await _serviceManager.ContactService.GetContactsByUserIdAsync(contactParams);

            // assert
            _cacheMock.Verify();
            _repositoryMock.Verify();
            _mapperMock.Verify();
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<ContactDto>>(result);
        }

        [Fact]
        public async Task InsertOrUpdateContactAsync_HasNoExistingContactAndContactInsertSuccess_ReturnsTrue()
        {
            // arrange
            ContactForCreationDto contactForCreationDto = ContactDtoFactory.CreateContactForCreationDto();
            Contact contactToSave = ContactDtoFactory.CreateContact();
            Contact existingContact = ContactDtoFactory.CreateContact();
            _mapperMock.Setup(m => m.Map<Contact>(It.IsAny<ContactForCreationDto>())).Returns(contactToSave).Verifiable();
            _repositoryMock.Setup(r => r.Contact.GetContactByUserIdContactIdAsync(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            _repositoryMock.Setup(r => r.Contact.InsertContactAsync(contactToSave)).ReturnsAsync(existingContact).Verifiable();

            // act 
            var result = await _serviceManager.ContactService.InsertOrUpdateContactAsync(contactForCreationDto);

            // assert
            _mapperMock.Verify();
            _repositoryMock.Verify();
            Assert.True(result);
        }

        [Fact]
        public async Task InsertOrUpdateContactAsync_HasNoExistingContactAndContactInsertFailed_ReturnsFalse()
        {
            // arrange
            ContactForCreationDto contactForCreationDto = ContactDtoFactory.CreateContactForCreationDto();
            Contact contactToSave = ContactDtoFactory.CreateContact();
            Contact existingContact = ContactDtoFactory.CreateContact();
            _mapperMock.Setup(m => m.Map<Contact>(It.IsAny<ContactForCreationDto>())).Returns(contactToSave).Verifiable();
            _repositoryMock.Setup(r => r.Contact.GetContactByUserIdContactIdAsync(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            _repositoryMock.Setup(r => r.Contact.InsertContactAsync(contactToSave)).Verifiable();

            // act 
            var result = await _serviceManager.ContactService.InsertOrUpdateContactAsync(contactForCreationDto);

            // assert
            _mapperMock.Verify();
            _repositoryMock.Verify();
            Assert.False(result);
        }

        [Fact]
        public async Task InsertOrUpdateContactAsync_HasExistingContactsAndUpdateFailed_ReturnsFalse()
        {
            // arrange
            ContactForCreationDto contactForCreationDto = ContactDtoFactory.CreateContactForCreationDto();
            Contact contactToSave = ContactDtoFactory.CreateContact();
            Contact existingContact = ContactDtoFactory.CreateContact();
            _mapperMock.Setup(m => m.Map<Contact>(It.IsAny<ContactForCreationDto>())).Returns(contactToSave).Verifiable();
            _repositoryMock.Setup(r => r.Contact.GetContactByUserIdContactIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(existingContact).Verifiable();
            _repositoryMock.Setup(r => r.Contact.UpdateContactStatusAsync(contactToSave)).ReturnsAsync(0).Verifiable();

            // act 
            var result = await _serviceManager.ContactService.InsertOrUpdateContactAsync(contactForCreationDto);

            // assert
            _mapperMock.Verify();
            _repositoryMock.Verify();
            Assert.False(result);
        }

        [Fact]
        public async Task InsertOrUpdateContactAsync_HasExistingContactsAndUpdateSuccess_ReturnsTrue()
        {
            // arrange
            ContactForCreationDto contactForCreationDto = ContactDtoFactory.CreateContactForCreationDto();
            Contact contactToSave = ContactDtoFactory.CreateContact();
            Contact existingContact = ContactDtoFactory.CreateContact();
            _mapperMock.Setup(m => m.Map<Contact>(It.IsAny<ContactForCreationDto>())).Returns(contactToSave).Verifiable();
            _repositoryMock.Setup(r => r.Contact.GetContactByUserIdContactIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(existingContact).Verifiable();
            _repositoryMock.Setup(r => r.Contact.UpdateContactStatusAsync(contactToSave)).ReturnsAsync(1).Verifiable();
            _cacheMock.Setup(c => c.RemoveDataAsync(It.IsAny<string>())).Verifiable(Times.Exactly(2));

            // act 
            var result = await _serviceManager.ContactService.InsertOrUpdateContactAsync(contactForCreationDto);

            // assert
            _mapperMock.Verify();
            _repositoryMock.Verify();
            _cacheMock.Verify();
            Assert.True(result);
        }

        [Fact]
        public async Task SearchContactsByNameUserIdAsync_NameIsNotNullOrEmpty_ReturnsUserDtoList()
        {
            // arrange
            ContactParameters contactParameters = new() { Name = "Test Name", UserId = 1, PageNumber = 1 };
            IEnumerable<User> users = [
                UserDtoFactory.CreateUser(userId: 2),
                UserDtoFactory.CreateUser(userId: 3),
                UserDtoFactory.CreateUser(userId: 4),
                ];
            IEnumerable<UserDto> userDtos = [
                UserDtoFactory.CreateUserDto(userId: 2),
                UserDtoFactory.CreateUserDto(userId: 3),
                UserDtoFactory.CreateUserDto(userId: 4),
                ];
            IEnumerable<Contact> contacts = [
                ContactDtoFactory.CreateContact(userId: 1, contactId:2),
                ContactDtoFactory.CreateContact(userId: 1, contactId:3),
                ContactDtoFactory.CreateContact(userId: 1, contactId:4)
                ];
            _repositoryMock.Setup(r => r.Contact.SearchContactsByNameUserIdAsync(It.IsAny<ContactParameters>())).ReturnsAsync(users).Verifiable();
            _mapperMock.Setup(m => m.Map<IEnumerable<UserDto>>(users)).Returns(userDtos).Verifiable(Times.Once);

            // act 
            var result = await _serviceManager.ContactService.SearchContactsByNameUserIdAsync(contactParameters);

            // assert
            _repositoryMock.Verify();
            _mapperMock.Verify();
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<UserDto>>(result);
        }

        [Fact]
        public async Task SearchContactsByNameUserIdAsync_ContactsAreOnCache_ReturnsUserDtoList()
        {
            // arrange
            ContactParameters contactParameters = new() { Name = "", UserId = 1, PageNumber = 1 };
            IEnumerable<User> users = [
                UserDtoFactory.CreateUser(userId: 2),
                UserDtoFactory.CreateUser(userId: 3),
                UserDtoFactory.CreateUser(userId: 4),
                ];
            IEnumerable<UserDto> userDtos = [
                UserDtoFactory.CreateUserDto(userId: 2),
                UserDtoFactory.CreateUserDto(userId: 3),
                UserDtoFactory.CreateUserDto(userId: 4),
                ];
            IEnumerable<Contact> contacts = [
                ContactDtoFactory.CreateContact(userId: 1, contactId:2),
                ContactDtoFactory.CreateContact(userId: 1, contactId:3),
                ContactDtoFactory.CreateContact(userId: 1, contactId:4)
                ];
            _cacheMock.Setup(c => c.GetCachedDataAsync<IEnumerable<User>>(It.IsAny<string>())).ReturnsAsync(users).Verifiable();
            _mapperMock.Setup(m => m.Map<IEnumerable<UserDto>>(users)).Returns(userDtos).Verifiable(Times.Once);

            // act 
            var result = await _serviceManager.ContactService.SearchContactsByNameUserIdAsync(contactParameters);

            // assert
            _cacheMock.Verify();
            _mapperMock.Verify();
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<UserDto>>(result);
        }

        [Fact]
        public async Task SearchContactsByNameUserIdAsync_ContactsAreOnDbAndPageIsOne_ReturnsUserDtoList()
        {
            // arrange
            ContactParameters contactParameters = new() { Name = "", UserId = 1, PageNumber = 1 };
            IEnumerable<User> users = [
                UserDtoFactory.CreateUser(userId: 2),
                UserDtoFactory.CreateUser(userId: 3),
                UserDtoFactory.CreateUser(userId: 4),
                ];
            IEnumerable<UserDto> userDtos = [
                UserDtoFactory.CreateUserDto(userId: 2),
                UserDtoFactory.CreateUserDto(userId: 3),
                UserDtoFactory.CreateUserDto(userId: 4),
                ];
            IEnumerable<Contact> contacts = [
                ContactDtoFactory.CreateContact(userId: 1, contactId:2),
                ContactDtoFactory.CreateContact(userId: 1, contactId:3),
                ContactDtoFactory.CreateContact(userId: 1, contactId:4)
                ];
            _cacheMock.Setup(c => c.GetCachedDataAsync<IEnumerable<User>>(It.IsAny<string>())).Verifiable();
            _repositoryMock.Setup(r => r.Contact.GetContactsByUserIdAsync(It.IsAny<ContactParameters>())).ReturnsAsync(contacts).Verifiable();
            _repositoryMock.Setup(r => r.User.GetUsersByIdsAsync(It.IsAny<string>())).ReturnsAsync(users).Verifiable();
            _mapperMock.Setup(m => m.Map<IEnumerable<UserDto>>(users)).Returns(userDtos).Verifiable();
            _cacheMock.Setup(c => c.SetCachedDataAsync(It.IsAny<string>(), It.IsAny<IEnumerable<User>>(), TimeSpan.FromMinutes(30))).Verifiable();

            // act 
            var result = await _serviceManager.ContactService.SearchContactsByNameUserIdAsync(contactParameters);

            // assert
            _cacheMock.Verify();
            _repositoryMock.Verify();
            _mapperMock.Verify();
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<UserDto>>(result);
        }

        [Fact]
        public async Task SearchContactsByNameUserIdAsync_ContactsAreOnDbAndPageIsNotOne_ReturnsUserDtoList()
        {
            // arrange
            ContactParameters contactParameters = new() { Name = "", UserId = 1, PageNumber = 2 };
            IEnumerable<User> users = [
                UserDtoFactory.CreateUser(userId: 2),
                UserDtoFactory.CreateUser(userId: 3),
                UserDtoFactory.CreateUser(userId: 4),
                ];
            IEnumerable<UserDto> userDtos = [
                UserDtoFactory.CreateUserDto(userId: 2),
                UserDtoFactory.CreateUserDto(userId: 3),
                UserDtoFactory.CreateUserDto(userId: 4),
                ];
            IEnumerable<Contact> contacts = [
                ContactDtoFactory.CreateContact(userId: 1, contactId:2),
                ContactDtoFactory.CreateContact(userId: 1, contactId:3),
                ContactDtoFactory.CreateContact(userId: 1, contactId:4)
                ];
            _cacheMock.Setup(c => c.GetCachedDataAsync<IEnumerable<User>>(It.IsAny<string>())).Verifiable();
            _repositoryMock.Setup(r => r.Contact.GetContactsByUserIdAsync(It.IsAny<ContactParameters>())).ReturnsAsync(contacts).Verifiable();
            _repositoryMock.Setup(r => r.User.GetUsersByIdsAsync(It.IsAny<string>())).ReturnsAsync(users).Verifiable();
            _mapperMock.Setup(m => m.Map<IEnumerable<UserDto>>(users)).Returns(userDtos).Verifiable(Times.Once);

            // act 
            var result = await _serviceManager.ContactService.SearchContactsByNameUserIdAsync(contactParameters);

            // assert
            _cacheMock.Verify();
            _repositoryMock.Verify();
            _mapperMock.Verify();
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<UserDto>>(result);
        }

        [Fact]
        public async Task InsertContactsAsync_UserIdIsLessThanZero_ThrowsInvalidParameterException()
        {
            // arrange
            int userId = 0;
            List<int> contactIds = [2, 3, 4, 5];

            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ContactService.InsertContactsAsync(userId, contactIds));

            // assert
            Assert.NotNull(result);
            Assert.Equal("The contact id's and user id provided is invalid.", result.Message);
        }

        [Fact]
        public async Task InsertContactsAsync_ContactIdsHasLessThanZero_ThrowsInvalidParameterException()
        {
            // arrange
            int userId = 1;
            List<int> contactIds = [2, 3, 4, 0];

            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ContactService.InsertContactsAsync(userId, contactIds));

            // assert
            Assert.NotNull(result);
            Assert.Equal("The contact id's and user id provided is invalid.", result.Message);
        }

        [Fact]
        public async Task InsertContactsAsync_InsertedContactsCountIsDifferentFromIntended_ThrowsInsertedContactRowsMismatchException()
        {
            // arrange
            int userId = 1;
            List<int> contactIds = [2, 3, 4, 5];
            IEnumerable<Contact> chatContacts = [
                ContactDtoFactory.CreateContact(userId:userId, contactId:2),
                ContactDtoFactory.CreateContact(userId:userId, contactId:3),
                ContactDtoFactory.CreateContact(userId:userId, contactId:4),
                ];
            IEnumerable<ContactDto> chatContactsToReturn = [
                ContactDtoFactory.CreateContactDto(userId:userId, contactId:2),
                ContactDtoFactory.CreateContactDto(userId:userId, contactId:3),
                ContactDtoFactory.CreateContactDto(userId:userId, contactId:4)
                ];
            _repositoryMock.Setup(r => r.Contact.InsertContactsAsync(It.IsAny<int>(), It.IsAny<List<int>>())).ReturnsAsync(chatContacts).Verifiable();
            _mapperMock.Setup(m => m.Map<IEnumerable<ContactDto>>(It.IsAny<IEnumerable<Contact>>())).Returns(chatContactsToReturn).Verifiable();

            // act
            var result = await Assert.ThrowsAsync< InsertedContactRowsMismatchException >(async () => await _serviceManager.ContactService.InsertContactsAsync(userId, contactIds));

            // assert
            _repositoryMock.Verify();
            _mapperMock.Verify();
            Assert.NotNull(result);
            Assert.Equal($"There was an issue adding the contacts to the chatroom. Total inserted ids: {chatContactsToReturn.Count()}; Total expected ids to insert: {contactIds.Count()}.", result.Message);
        }

        [Fact]
        public async Task InsertContactsAsync_Default_ReturnsContactDto()
        {
            // arrange
            int userId = 1;
            List<int> contactIds = [2,3,4,5];
            IEnumerable<Contact> chatContacts = [
                ContactDtoFactory.CreateContact(userId:userId, contactId:2),
                ContactDtoFactory.CreateContact(userId:userId, contactId:3),
                ContactDtoFactory.CreateContact(userId:userId, contactId:4),
                ContactDtoFactory.CreateContact(userId:userId, contactId:5)
                ];
            IEnumerable<ContactDto> chatContactsToReturn = [
                ContactDtoFactory.CreateContactDto(userId:userId, contactId:2),
                ContactDtoFactory.CreateContactDto(userId:userId, contactId:3),
                ContactDtoFactory.CreateContactDto(userId:userId, contactId:4),
                ContactDtoFactory.CreateContactDto(userId:userId, contactId:5)
                ];
            _repositoryMock.Setup(r => r.Contact.InsertContactsAsync(It.IsAny<int>(), It.IsAny<List<int>>())).ReturnsAsync(chatContacts).Verifiable();
            _mapperMock.Setup(m => m.Map<IEnumerable<ContactDto>>(It.IsAny<IEnumerable<Contact>>())).Returns(chatContactsToReturn).Verifiable();
            _cacheMock.Setup(c => c.RemoveDataAsync(It.IsAny<string>())).Verifiable();

            // act
            var result = await _serviceManager.ContactService.InsertContactsAsync(userId, contactIds);

            // assert
            _repositoryMock.Verify();
            _mapperMock.Verify();
            _cacheMock.Verify();
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<ContactDto>>(result);
        }

    }
}
