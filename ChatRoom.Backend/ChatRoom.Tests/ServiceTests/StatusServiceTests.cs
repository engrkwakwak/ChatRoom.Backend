using AutoMapper;
using ChatRoom.Backend;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using FluentAssertions;
using Moq;
using RedisCacheService;
using Service;
using Shared.DataTransferObjects.Status;

namespace ChatRoom.UnitTest.ServiceTests; 
public class StatusServiceTests {
    private readonly Mock<IRepositoryManager> _mockRepo;
    private readonly Mock<ILoggerManager> _mockLogger;
    private readonly Mock<IRedisCacheManager> _mockCache;
    private readonly StatusService _service;

    public StatusServiceTests() {
        _mockCache = new Mock<IRedisCacheManager>();
        _mockLogger = new Mock<ILoggerManager>();
        _mockRepo = new Mock<IRepositoryManager>();

        var mappingConfig = new MapperConfiguration(mc => {
            mc.AddProfile(new MappingProfile());
        });
        var mapper = mappingConfig.CreateMapper();

        _service = new StatusService(
            _mockRepo.Object, _mockLogger.Object, 
            mapper, _mockCache.Object
        );
    }

    [Fact]
    public async Task GetStatusByIdAsync_StatusExistsInCache_ShouldReturnStatusDto() {
        // Arrange
        Status status = CreateStatus();

        _mockCache.Setup(x => x.GetCachedDataAsync<Status>(It.IsAny<string>()))
            .ReturnsAsync(status);

        // Act
        var result = await _service.GetStatusByIdAsync(status.StatusId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<StatusDto>();
    }

    [Fact]
    public async Task GetStatusByIdAsync_StatusExistsInCache_ShouldMapStatusToStatusDtoCorrectly() {
        // Arrange
        Status status = CreateStatus();

        _mockCache.Setup(x => x.GetCachedDataAsync<Status>(It.IsAny<string>()))
            .ReturnsAsync(status);

        // Act
        var result = await _service.GetStatusByIdAsync(status.StatusId);

        // Assert
        result.StatusId.Should().Be(status.StatusId);
        result.StatusName.Should().Be(status.StatusName);
    }

    [Fact]
    public async Task GetStatusByIdAsync_StatusExistsInCache_ShouldNotFetchStatusFromDatabase() {
        // Arrange
        Status status = CreateStatus();

        _mockCache.Setup(x => x.GetCachedDataAsync<Status>(It.IsAny<string>()))
            .ReturnsAsync(status);

        // Act
        var result = await _service.GetStatusByIdAsync(status.StatusId);

        // Assert
        _mockRepo.Verify(x => x.Status.GetStatusByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetStatusByIdAsync_StatusDoesNotExistInDatabase_ShouldThrowNotFoundException() {
        // Arrange
        Status status = CreateStatus();

        _mockCache.Setup(x => x.GetCachedDataAsync<Status?>(It.IsAny<string>()))
            .ReturnsAsync((Status?)null);
        _mockRepo.Setup(x => x.Status.GetStatusByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new StatusIdNotFoundException(status.StatusId));

        // Act
        Func<Task> act = async() => await _service.GetStatusByIdAsync(status.StatusId);

        // Assert
        await act.Should().ThrowAsync<StatusIdNotFoundException>()
            .WithMessage($"The status with id: {status.StatusId} doesn't exists in the database.");
    }

    [Fact]
    public async Task GetStatusByIdAsync_StatusExistsInDatabase_ShouldReturnStatusDto() {
        // Arrange
        Status status = CreateStatus();

        _mockCache.Setup(x => x.GetCachedDataAsync<Status?>(It.IsAny<string>()))
            .ReturnsAsync((Status?)null);
        _mockRepo.Setup(x => x.Status.GetStatusByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(status);

        // Act
        var result = await _service.GetStatusByIdAsync(status.StatusId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<StatusDto>();
    }

    [Fact]
    public async Task GetStatusByIdAsync_StatusExistsInDatabase_ShouldMapStatusToStatusDtoCorrectly() {
        // Arrange
        Status status = CreateStatus();

        _mockCache.Setup(x => x.GetCachedDataAsync<Status?>(It.IsAny<string>()))
            .ReturnsAsync((Status?)null);
        _mockRepo.Setup(x => x.Status.GetStatusByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(status);

        // Act
        var result = await _service.GetStatusByIdAsync(status.StatusId);

        // Assert
        result.StatusId.Should().Be(status.StatusId);
        result.StatusName.Should().Be(status.StatusName);
    }

    private static Status CreateStatus() {
        return new Status() {
            StatusId = 1,
            StatusName = "Test"
        };
    }
}
