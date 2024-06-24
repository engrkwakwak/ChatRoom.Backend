using ChatRoom.Backend.Presentation.Controllers;
using ChatRoom.Backend.Presentation.Hubs;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Moq;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;

namespace ChatRoom.UnitTest.ControllerTests
{
    public class AuthControllerTests
    {
        private readonly Mock<IServiceManager> _serviceMock = new();

        private readonly Mock<IConfiguration> _configurationMock =  new();

        private readonly AuthController _controller;

        public AuthControllerTests() 
        { 
            _controller = new AuthController(_serviceMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task Authenticate_InvalidInputs_ReturnsValidationError()
        {
            SignInDto user = new()
            {

            };
            _serviceMock.Setup(s => s.AuthService.ValidateUser(user)).ReturnsAsync(true);

            IActionResult result = await _controller.Authenticate(user);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Authenticate_ValidateUserIsFalse_ReturnsUnauthorizedStatus()
        {
            SignInDto user = new()
            {

            };
            _serviceMock.Setup(s => s.AuthService.ValidateUser(user)).ReturnsAsync(false);

            var result = await _controller.Authenticate(user);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void Authenticate_ValidateUserIsTrue_ReturnsOkStatus()
        {
            Assert.Fail();
        }
    }
}
