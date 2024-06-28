using AutoMapper;
using Contracts;
using Microsoft.Extensions.Configuration;
using Moq;
using RedisCacheService;
using Service;
using Service.Contracts;

namespace ChatRoom.UnitTest.ServiceTests
{
    public class AuthServiceTests
    {
        private readonly Mock<IRepositoryManager> _repositoryMock = new();
        private readonly Mock<ILoggerManager> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly Mock<IRedisCacheManager> _cacheMock = new();
        private readonly Mock<IFileManager> _fileManagerMock = new();
        private readonly IServiceManager _serviceManager;

        public AuthServiceTests()
        {
            _serviceManager = new ServiceManager(_repositoryMock.Object, _loggerMock.Object, _mapperMock.Object, _configurationMock.Object, _cacheMock.Object, _fileManagerMock.Object);
        }

        [Fact]
        public void Test1()
        {

        }
    }
}
