using ChatRoom.Backend.Presentation.Controllers;
using Microsoft.Extensions.Configuration;
using Moq;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatRoom.UnitTest.ControllerTests
{
    public class ChatsControllerTests
    {
        private readonly Mock<IServiceManager> _serviceMock = new();

        private readonly Mock<IConfiguration> _configurationMock = new();

        private readonly ChatsController _controller;
    }
}
